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
    public GameObject currentCharacter => GameManager._instance.CurrentCharacter;
    private int currentCharacterIndex;

    public GameObject pharaoh => GameManager._instance.Pharaoh;
    public GameObject priest => GameManager._instance.Priest;

    //CameraControl
    private GameObject mainCam;
    private float postProcessWeight;

    //AbilitySupport
    //[HideInInspector]
    public GameObject targetObject;

    public InGameMenu canvas;

    public bool[] priestAbilities;
    public bool[] pharaohAbilities;

    public UnitInteractions sn;
    


    // Start is called before the first frame update
    void Start()
    {
        Initialize();

        if (NetworkManager._instance.IsHost)
        {
            GameManager._instance.SelectCharacter(ObjectType.pharaoh);
        }
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
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (NetworkManager._instance.IsHost)
            {
                GameManager._instance.SelectCharacter(ObjectType.priest);
                sn.SelectPriestUI();
            }
            else
            {
                if (NetworkManager._instance.ShouldSendToServer)
                {
                    ClientSend.SelectCharacterRequest(ObjectType.priest);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (NetworkManager._instance.IsHost)
            {
                GameManager._instance.SelectCharacter(ObjectType.pharaoh);
                sn.SelectPharaohUI();
            }
            else
            {
                if (NetworkManager._instance.ShouldSendToServer)
                {
                    ClientSend.SelectCharacterRequest(ObjectType.pharaoh);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            if (NetworkManager._instance.IsHost)
            {
                GameManager._instance.UnselectCharacter();
                sn.UnselectCharacter();
            }
            else
            {
                if (NetworkManager._instance.ShouldSendToServer)
                {
                    ClientSend.UnselectCharacterRequest();
                }
            }
        }
    }

    public void UpdateAbilities()
    {
        pharaoh.GetComponent<PlayerController>().abilityAllowed = pharaohAbilities;
        priest.GetComponent<PlayerController>().abilityAllowed = priestAbilities;
    }

}
