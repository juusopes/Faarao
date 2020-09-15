using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InGameMenu : MonoBehaviour
{
    public GameObject menuPanel, optionsPanel, audioPanel, videoPanel;
    public GameObject continueButton, loadButton, saveButton, optionsButton, restartButton, mainMenuButton;
    public GameObject gameplayButton, audioButton, controlsButton, videoButton;

    public Text timeText;
    private float startTime;

    public bool menuActive;

    int hours = 0;
    int minutes = 0;
    int seconds = 0;

    // Start is called before the first frame update
    void Start()
    {
        menuActive = false;

        menuPanel.SetActive(false);
        optionsPanel.SetActive(false);
        audioPanel.SetActive(false);
        videoPanel.SetActive(false);

        startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        timer();

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
            videoPanel.SetActive(false);
        }
    }

    public void timer()
    {
        float t = Time.time - startTime;

        string hours = ((int)t / 3600).ToString();
        string minutes = ((int)t / 60).ToString();
        string seconds = (t % 60).ToString("f1");

        timeText.text = "Level time: " + hours + ":" + minutes + ":" + seconds;
    }
}
