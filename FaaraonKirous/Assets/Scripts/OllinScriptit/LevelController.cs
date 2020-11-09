using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class LevelController : MonoBehaviour
{
    //Switch Character
    private GameObject[] characters;
    public GameObject currentCharacter;
    private int currentCharacterIndex;
    public GameObject playerOneImage, playerTwoImage;

    [SerializeField]
    private GameObject pharaoh;
    [SerializeField]
    private GameObject priest;

    //CameraControl
    private GameObject mainCam;
    private float postProcessWeight;

    //AbilitySupport
    //[HideInInspector]
    public GameObject targetObject;

    public InGameMenu canvas;


    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        KeyBoardControls();
        // TODO: Doesn't work if no activeCharacter
        //InivsibilityView();
    }

    private void Initialize()
    {
        characters = GameObject.FindGameObjectsWithTag("Player");
        mainCam = GameObject.FindGameObjectWithTag("MainCamera");
        mainCam = mainCam.transform.parent.gameObject;

        canvas.gameObject.SetActive(true);
        playerOneImage.SetActive(true);

        if (NetworkManager._instance.IsHost)
        {
            //SetACtiveCharacter
            currentCharacterIndex = 0;
            foreach (GameObject character in characters)
            {
                character.GetComponent<PlayerController>().IsCurrentPlayer = false;
                if (character.GetComponent<PlayerController>().playerOne)
                {
                    currentCharacter = character;
                    character.GetComponent<PlayerController>().IsCurrentPlayer = true;
                    character.GetComponent<PlayerController>().IsActivePlayer = true;
                }
                if (currentCharacter == null)
                {
                    currentCharacterIndex++;
                }
            }
            if (currentCharacter == null)
            {
                currentCharacterIndex = 0;
                characters[currentCharacterIndex].GetComponent<PlayerController>().IsCurrentPlayer = true;
                characters[currentCharacterIndex].GetComponent<PlayerController>().IsActivePlayer = true;
                currentCharacter = characters[currentCharacterIndex];
            }
        }

        //SetCameraPos
        //mainCam.transform.parent = activeCharacter.transform;
    }

    private void InivsibilityView()
    {
        if (currentCharacter.GetComponent<PlayerController>().isInvisible)
        {
            if (postProcessWeight <= 1)
            {
                postProcessWeight += Time.deltaTime;
            }
        } else
        {
            if (postProcessWeight >= 0)
            {
                postProcessWeight -= Time.deltaTime;
            }
        }
        mainCam.transform.GetChild(0).GetComponent<PostProcessVolume>().weight = postProcessWeight;
    }
    

    //ButtonInterface
    public void ActiveCharacterAttack()
    {
        currentCharacter.GetComponent<PlayerController>().Attack();
    }
    //public void ActiveCharacterAbility()
    //{
    //    activeCharacter.GetComponent<PlayerController>().UseAbility();
    //}
    public void ActiveCharacterCrouch()
    {
        currentCharacter.GetComponent<PlayerController>().Crouch();
    }
    public void ActiveCharacterInteract()
    {
        currentCharacter.GetComponent<PlayerController>().Interact();
    }
    public void ActiveCharacterStay()
    {
        currentCharacter.GetComponent<PlayerController>().Stay();
    }


    private void KeyBoardControls()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            SelectCharacter(ObjectType.priest);
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            SelectCharacter(ObjectType.pharaoh);
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            UnselectCharacter();
        }
    }

    public void UnselectCharacter()
    {
        if (currentCharacter == null) return;

        currentCharacter.GetComponent<PlayerController>().IsCurrentPlayer = false;

        if (NetworkManager._instance.IsHost)
        {
            currentCharacter.GetComponent<PlayerController>().IsActivePlayer = false;
        }
        else
        {
            if (NetworkManager._instance.ShouldSendToServer)
            {
                // TODO: Send unselect packet (pass id)
            }
        }

        currentCharacter = null;
    }

    public void SelectCharacter(ObjectType character)
    {
        if (NetworkManager._instance.IsHost)
        {
            GameObject obj;
            switch (character)
            {
                case ObjectType.pharaoh:
                    obj = pharaoh;
                    break;
                case ObjectType.priest:
                    obj = priest;
                    break;
                default:
                    return;
            }

            PlayerController playerController = obj.GetComponent<PlayerController>();
            if (!playerController.IsActivePlayer)
            {
                if (currentCharacter != null)
                {
                    currentCharacter.GetComponent<PlayerController>().IsActivePlayer = false;
                    currentCharacter.GetComponent<PlayerController>().IsCurrentPlayer = false;
                }

                playerController.IsActivePlayer = true;
                playerController.IsCurrentPlayer = true;
                currentCharacter = obj;
            }
        }
        else
        {
            if (NetworkManager._instance.ShouldSendToServer)
            {
                // TODO: Send select character request
            }
        }
    }

    public void SwitchCharacter()
    {
        //Switch Player
        characters[currentCharacterIndex].GetComponent<PlayerController>().IsCurrentPlayer = false;
        currentCharacterIndex++;
        if (currentCharacterIndex > characters.Length - 1)
        {
            currentCharacterIndex = 0;
        }
        currentCharacter = characters[currentCharacterIndex];
        characters[currentCharacterIndex].GetComponent<PlayerController>().IsCurrentPlayer = true;
        mainCam.GetComponent<CameraControl>().activeCharacter = currentCharacter;
        if (mainCam.GetComponent<CameraControl>().camFollow)
        {
            mainCam.transform.parent = currentCharacter.transform;
        }

        //Set UI elements
        if (currentCharacterIndex == 1)
        {
            playerOneImage.SetActive(false);
            playerTwoImage.SetActive(true);
        }
        else
        {
            playerOneImage.SetActive(true);
            playerTwoImage.SetActive(false);
        }

    }
}
