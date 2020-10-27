using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class InGameMenu : MonoBehaviour
{
    public GameObject menuPanel, optionsPanel, audioPanel, videoPanel, controlsPanel, gameplayPanel;
    public GameObject continueButton, loadButton, saveButton, optionsButton, restartButton, mainMenuButton;
    public GameObject gameplayButton, audioButton, controlsButton, videoButton;
    public int savedLevel;
    public float lastSaveSpotX, lastSaveSpotY, lastSaveSpotZ;
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
        //TODO: fix below
        if(player)
            player.transform.position = new Vector3(lastSaveSpotX, lastSaveSpotY, lastSaveSpotZ);
        if(player2)
            player2.transform.position = new Vector3(lastSaveSpotX2, lastSaveSpotY2, lastSaveSpotZ2);
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
        SceneManager.LoadScene("OllinScene");
    }

    public void RestartLevel()
    {
        string restartLevel = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(restartLevel);
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void SaveLevel()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/LevelInfo.dat");
        LevelData data = new LevelData();

        print("Level saved at: " + player.transform.position);

        data.lastSaveSpotX = player.transform.position.x;
        data.lastSaveSpotY = player.transform.position.y;
        data.lastSaveSpotZ = player.transform.position.z;

        data.lastSaveSpotX2 = player2.transform.position.x;
        data.lastSaveSpotY2 = player2.transform.position.y;
        data.lastSaveSpotZ2 = player2.transform.position.z;

        string savedLevel = SceneManager.GetActiveScene().name;
        data.savedLevel = savedLevel;

        bf.Serialize(file, data);

        file.Close();
    }

    public void LoadLevel()
    {
        if (File.Exists(Application.persistentDataPath + "/LevelInfo.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/LevelInfo.dat", FileMode.Open);
            LevelData data = (LevelData)bf.Deserialize(file);

            print("Level loaded at: " + player.transform.position);

            file.Close();

            player.transform.position = new Vector3(data.lastSaveSpotX, data.lastSaveSpotY, data.lastSaveSpotZ);
            player2.transform.position = new Vector3(data.lastSaveSpotX2, data.lastSaveSpotY2, data.lastSaveSpotZ2);
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
