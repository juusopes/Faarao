using System.Collections;
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

    //[HideInInspector]
    public GameObject menuPanel, optionsPanel, audioPanel, videoPanel, controlsPanel, gameplayPanel, background;
    [HideInInspector]
    public GameObject continueButton, loadButton, saveButton, optionsButton, restartButton, mainMenuButton;
    [HideInInspector]
    public GameObject gameplayButton, audioButton, controlsButton, videoButton;

    // TODO: For testing
    public GameObject objectivePanel;

    public GameObject fadeFromBlack;

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

        fadeFromBlack.SetActive(true);
        menuPanel.SetActive(false);
        optionsPanel.SetActive(false);
        audioPanel.SetActive(false);
        videoPanel.SetActive(false);
        controlsPanel.SetActive(false);
        gameplayPanel.SetActive(false);

        startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        Scene sceneName = SceneManager.GetActiveScene();

        if (sceneName.name != "MainMenuAnimated")
        {
            timer();

            if (DontDestroyCanvas.Instance.IsOpen())
            {
                return;
            }

            if (Input.GetButtonDown("Cancel") && !menuActive)
            {
                ActivateMenu();
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

    public void ActivateMenu()
    {
        menuActive = true;

        background.SetActive(true);
        menuPanel.SetActive(true);
        objectivePanel.SetActive(false);
    }

    public void DeactivateMenu()
    {
        menuActive = false;

        background.SetActive(false);
        menuPanel.SetActive(false);
        optionsPanel.SetActive(false);
        audioPanel.SetActive(false);
        videoPanel.SetActive(false);
        controlsPanel.SetActive(false);
        gameplayPanel.SetActive(false);
    }

    public void NewGame()
    {
        GameManager._instance.NewGame();
    }

    public void RestartLevel()
    {
        if (NetworkManager._instance.IsHost)
        {
            GameManager._instance.LoadLevel(GameManager._instance.CurrentSceneIndex);
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
        if (NetworkManager._instance.IsHost)
        {
            GameManager._instance.ExitToMainMenu();
        }
    }

    public void SaveLevel()
    {
        if (NetworkManager._instance.IsHost)
        {
            SaveUIManager.Instance.OpenInGameMenu();
        }
    }

    public void Quickload()
    {
        if (NetworkManager._instance.IsHost)
        {
            GameManager._instance.LoadFromFile();
        }
    }

    public void Quicksave()
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
            LoadUIManager.Instance.OpenInGameMenu();
        }
    }

    public void LoadLevelMainMenu()
    {
        MainMenuUIManager.Instance.Close();
        LoadUIManager.Instance.OpenMainMenu();
    }

    public void Host()
    {
        if (NetworkManager._instance.IsHost)
        {
            HostUIManager.Instance.OpenInGameMenu();
        }
    }

    public void ConnectToGame()
    {

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
