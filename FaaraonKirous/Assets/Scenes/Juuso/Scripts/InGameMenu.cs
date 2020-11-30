﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class InGameMenu : MonoBehaviour
{
    public static InGameMenu _instance;

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
        }
    }

    [HideInInspector]
    public GameObject menuPanel, optionsPanel, audioPanel, videoPanel, controlsPanel, gameplayPanel;
    [HideInInspector]
    public GameObject continueButton, loadButton, saveButton, optionsButton, restartButton, mainMenuButton;
    [HideInInspector]
    public GameObject gameplayButton, audioButton, controlsButton, videoButton;

    // TODO: For testing
    public GameObject objectivePanel;

    public int savedLevel;
    [HideInInspector]
    public float lastSaveSpotX, lastSaveSpotY, lastSaveSpotZ;
    [HideInInspector]
    public float lastSaveSpotX2, lastSaveSpotY2, lastSaveSpotZ2;

    public Text timeText;
    private float startTime;

    public bool menuActive;

    //testejä varten
    public GameObject player, player2;

    // Start is called before the first frame update
    void Start()
    {
        menuActive = false;

        menuPanel.SetActive(false);
        optionsPanel.SetActive(false);
        audioPanel.SetActive(false);
        videoPanel.SetActive(false);
        controlsPanel.SetActive(false);
        gameplayPanel.SetActive(false);

        startTime = Time.time;

        if (player != null && player2 != null)
        {
            player.transform.position = new Vector3(lastSaveSpotX, lastSaveSpotY, lastSaveSpotZ);
            player2.transform.position = new Vector3(lastSaveSpotX2, lastSaveSpotY2, lastSaveSpotZ2);
        }
    }

    // Update is called once per frame
    void Update()
    {
        Scene sceneName = SceneManager.GetActiveScene();

        if (sceneName.name != "MainMenu")
        {
            timer();

            if (Input.GetButtonDown("Cancel") && !menuActive)
            {
                menuActive = true;

                menuPanel.SetActive(true);
                objectivePanel.SetActive(false);

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

            else if (Input.GetButtonDown("Cancel") && menuActive)
            {
                DeactivateMenu();
            }
        }

        else
        {
            menuPanel.SetActive(true);
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

    public void DeactivateMenu()
    {
        menuActive = false;

        menuPanel.SetActive(false);
        optionsPanel.SetActive(false);
        audioPanel.SetActive(false);
        videoPanel.SetActive(false);
        controlsPanel.SetActive(false);
        gameplayPanel.SetActive(false);
    }

    public void NewGame()
    {
        SceneManager.LoadScene(1);
    }

    public void RestartLevel()
    {
        if (NetworkManager._instance.IsHost)
        {
            int sceneIndex = SceneManager.GetActiveScene().buildIndex;
            GameManager._instance.LoadLevel(sceneIndex);
        }
    }

    public void EnableLoadingScreen()
    {
        // TODO: This is for testing only. Don't destroy on load canvas would be better
        objectivePanel.SetActive(true);
    }

    public void DisableLoadingScreen()
    {
        // TODO: This is for testing only. Don't destroy on load canvas would be better
        objectivePanel.SetActive(false);
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void SaveLevel()
    {
        if (NetworkManager._instance.IsHost)
        {
            GameManager._instance.SaveToFile();
        }
    }

    public void LoadLevel()
    {
        if (NetworkManager._instance.IsHost)
        {
            GameManager._instance.LoadFromFile();
        }
    }

    public void LoadLevelFromMenu()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(Application.persistentDataPath + "/LevelInfo.dat", FileMode.Open);
        LevelData data = (LevelData)bf.Deserialize(file);

        // atm vain yksi load slot
        SceneManager.LoadScene(data.savedLevel);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    [System.Serializable]
    class LevelData
    {
        public float lastSaveSpotX;
        public float lastSaveSpotY;
        public float lastSaveSpotZ;

        public float lastSaveSpotX2;
        public float lastSaveSpotY2;
        public float lastSaveSpotZ2;

        public string savedLevel;
    }
}
