using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class LevelController : MonoBehaviour
{
    public static LevelController _instance;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
            return;
        }
    }

    //Switch Character
    private GameObject[] characters;
    public GameObject currentCharacter;
    private int currentCharacterIndex;
    public GameObject playerOneImage, playerTwoImage;

    [SerializeField]
    public GameObject pharaoh;
    [SerializeField]
    public GameObject priest;

    //CameraControl
    private GameObject mainCam;
    private float postProcessWeight;

    //AbilitySupport
    //[HideInInspector]
    public GameObject targetObject;

    public InGameMenu canvas;

    public bool[] priestAbilities;
    public bool[] pharaohAbilities;

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
                    pharaoh = character;
                    character.GetComponent<PlayerController>().IsCurrentPlayer = true;
                    character.GetComponent<PlayerController>().IsActivePlayer = true;
                } else
                {
                    priest = character;
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

        UpdateAbilities();
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
            UnselectCurrentCharacter();
        }
    }

    public void UnselectCurrentCharacter()
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
                ObjectType character = currentCharacter.GetComponent<PlayerNetManager>().Type;
                ClientSend.UnselectCharacter(character);
            }
        }

        currentCharacter = null;
    }

    public void UnselectCharacter(ObjectType character)
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

        obj.GetComponent<PlayerController>().IsActivePlayer = false;
    }

    public void SelectCharacter(ObjectType character)
    {
        if (NetworkManager._instance.IsHost)
        {
            if (CanChangeToCharacter(character))
            {
                ChangeToCharacter(character);
            }
        }
        else
        {
            if (NetworkManager._instance.ShouldSendToServer)
            {
                ClientSend.ChangeToCharacterRequest(character);
            }
        }
    }

    public bool CanChangeToCharacter(ObjectType character)
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
                return false;
        }

        if (!obj.GetComponent<PlayerController>().IsActivePlayer)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void ChangeToCharacter(ObjectType character, bool isMe = true)
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

        if (isMe)
        {
            Debug.Log($"Changed to {character}");
            if (currentCharacter != null) UnselectCurrentCharacter();
            playerController.IsCurrentPlayer = true;
            currentCharacter = obj;
        }
        
        if (NetworkManager._instance.IsHost)
        {
            playerController.IsActivePlayer = true;
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
    public void UpdateAbilities()
    {
        pharaoh.GetComponent<PlayerController>().abilityAllowed = pharaohAbilities;
        priest.GetComponent<PlayerController>().abilityAllowed = priestAbilities;
    }

}
