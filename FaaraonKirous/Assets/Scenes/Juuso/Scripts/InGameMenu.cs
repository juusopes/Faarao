using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameMenu : MonoBehaviour
{
    public GameObject menuPanel, optionsPanel, audioPanel;
    public GameObject continueButton, loadButton, saveButton, optionsButton, restartButton, mainMenuButton;
    public GameObject gameplayButton, audioButton, controlsButton, videoButton;
    public AudioSource backgroundMusic;

    public bool menuActive;

    // Start is called before the first frame update
    void Start()
    {
        menuActive = false;

        menuPanel.SetActive(false);
        optionsPanel.SetActive(false);
        audioPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("Cancel") && !menuActive)
        {
            menuActive = true;

            menuPanel.SetActive(true);

            continueButton.SetActive(true);
            loadButton.SetActive(true);
            saveButton.SetActive(true);
            optionsButton.SetActive(true);
            restartButton.SetActive(true);
            mainMenuButton.SetActive(true);
            gameplayButton.SetActive(true);
            audioButton.SetActive(true);
            controlsButton.SetActive(true);
            videoButton.SetActive(true);
        }

        else if(Input.GetButtonDown("Cancel") && menuActive)
        {
            menuActive = false;

            menuPanel.SetActive(false);
            optionsPanel.SetActive(false);
            audioPanel.SetActive(false);
        }
    }

    //public void DisableChildren()
    //{
    //    foreach (Transform weapon in menuPanel.GetComponentsInChildren<Transform>())
    //    {
    //        for (int i = 0; i < menuPanel.transform.childCount; i++)
    //        {
    //            // deactivates other weapons and stances
    //            var child = menuPanel.transform.GetChild(i).gameObject;

    //            if (child != null && child.name.Contains("Button") /*|| child.name.Contains("Hand")*/)
    //            {
    //                child.SetActive(false);
    //            }
    //        }
    //    }
    //}
}
