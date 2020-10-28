﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class LevelController : MonoBehaviour
{
    //Switch Character
    private GameObject[] characters;
    public GameObject activeCharacter;
    private int currentCharacter;
    public GameObject playerOneImage, playerTwoImage;

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
        //objCount = 3;

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

        canvas.gameObject.SetActive(true);
        playerOneImage.SetActive(true);
        //SetACtiveCharacter
        currentCharacter = 0;
        foreach (GameObject character in characters)
        {
            character.GetComponent<PlayerController>().isActiveCharacter = false;
            if (character.GetComponent<PlayerController>().playerOne)
            {
                activeCharacter = character;
                character.GetComponent<PlayerController>().isActiveCharacter = true;
            }
            if (activeCharacter == null)
            {
                currentCharacter++;
            }
        }
        if (activeCharacter == null)
        {
            currentCharacter = 0;
            characters[currentCharacter].GetComponent<PlayerController>().isActiveCharacter = true;
            activeCharacter = characters[currentCharacter];
        }
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
        characters[currentCharacter].GetComponent<PlayerController>().isActiveCharacter = false;
        currentCharacter++;
        if (currentCharacter > characters.Length - 1)
        {
            currentCharacter = 0;
        }
        activeCharacter = characters[currentCharacter];
        characters[currentCharacter].GetComponent<PlayerController>().isActiveCharacter = true;
        mainCam.GetComponent<CameraControl>().activeCharacter = activeCharacter;
        if (mainCam.GetComponent<CameraControl>().camFollow)
        {
            mainCam.transform.parent = activeCharacter.transform;
        }

        //Set UI elements
        if (currentCharacter == 1)
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

    //ButtonInterface
    public void ActiveCharacterAttack()
    {
        activeCharacter.GetComponent<PlayerController>().Attack();
    }
    //public void ActiveCharacterAbility()
    //{
    //    activeCharacter.GetComponent<PlayerController>().UseAbility();
    //}
    public void ActiveCharacterCrouch()
    {
        activeCharacter.GetComponent<PlayerController>().Crouch();
    }
    public void ActiveCharacterInteract()
    {
        activeCharacter.GetComponent<PlayerController>().Interact();
    }
    public void ActiveCharacterStay()
    {
        activeCharacter.GetComponent<PlayerController>().Stay();
    }


    private void KeyBoardControls()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            SwitchCharacter();
        }
    }
}
