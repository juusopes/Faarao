using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UIElements;

public class LevelController : MonoBehaviour
{
    //Switch Character
    private GameObject[] characters;
    public GameObject activeCharacter;
    private int current;
    public GameObject playerOneImage, playerTwoImage;

    //CameraControl
    private GameObject mainCam;
    private float postProcessWeight;

    //AbilitySupport
    //[HideInInspector]
    public GameObject targetObject;

    public GameObject canvas;


    // Start is called before the first frame update
    void Start()
    {
        canvas.SetActive(true);
        playerOneImage.SetActive(true);

        Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        KeyBoardControls();
        InivsibilityView();
    }

    private void Initialize()
    {
        characters = GameObject.FindGameObjectsWithTag("Player");
        mainCam = GameObject.FindGameObjectWithTag("MainCamera");
        mainCam = mainCam.transform.parent.gameObject;
        //SetACtiveCharacter
        current = 0;
        foreach (GameObject character in characters)
        {
            character.GetComponent<PlayerController>().isActiveCharacter = false;
        }
        characters[current].GetComponent<PlayerController>().isActiveCharacter = true;
        activeCharacter = characters[current];
        //SetCameraPos
            mainCam.transform.parent = activeCharacter.transform;


    }

    private void InivsibilityView()
    {
        if (activeCharacter.GetComponent<PlayerController>().isInvisible)
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
    public void SwitchCharacter()
    {
        //Switch Player
        characters[current].GetComponent<PlayerController>().isActiveCharacter = false;
        current++;
        if (current > characters.Length - 1)
        {
            current = 0;
        }
        activeCharacter = characters[current];
        characters[current].GetComponent<PlayerController>().isActiveCharacter = true;
        mainCam.GetComponent<CameraControl>().activeCharacter = activeCharacter;
        if (mainCam.GetComponent<CameraControl>().camFollow)
        {
            mainCam.transform.parent = activeCharacter.transform;
        }

        //Set UI elements
        if (current == 1)
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

    private void KeyBoardControls()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            SwitchCharacter();
        }
    }
}
