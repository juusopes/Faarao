﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Permissions;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public enum ObjectList : byte
{
    enemy,
    player,
    waypointGroup,
    activatable,
    controller,
    objective,
    camera,
    count
}

public enum ObjectType : short
{
    enemy,
    pharaoh,
    priest,
    waypointGroup,
    activatable,
    objectiveController,
    camera,
    objective
}

public class GameManager : MonoBehaviour
{
    public static GameManager _instance;

    private readonly Dictionary<ObjectList, Dictionary<int, ObjectManager>> _objectLists = new Dictionary<ObjectList, Dictionary<int, ObjectManager>>();
    private readonly Dictionary<ObjectList, int> _counters = new Dictionary<ObjectList, int>();

    // Prefabs
    private readonly Dictionary<ObjectType, GameObject> _objectPrefabs = new Dictionary<ObjectType, GameObject>();
    [SerializeField]
    private GameObject _enemyClientPrefab = null;

    // Profile
    public string Name { get; private set; } = null;

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

        // TODO: Hosts name should be added later
        // Add default so that we don't get errors
        Players.Add(Constants.defaultConnectionId, new PlayerInfo
        {
            Name = "default"
        });

        // Add lists
        for (int i = 0; i < (int)ObjectList.count; ++i)
        {
            _objectLists.Add((ObjectList)i, new Dictionary<int, ObjectManager>());
        }

        // Add prefabs
        _objectPrefabs.Add(ObjectType.enemy, _enemyClientPrefab);

        // For testing
        if (NetworkManager._instance.Testing && NetworkManager._instance.IsHost)
        {
            IsSceneLoaded = true;
            IsFullyLoaded = true;
        }
    }

    private void Start()
    {
        
    }

    #region Loading/Saving
    /// <summary>Returns the build index of the current scene.</summary>
    public int CurrentSceneIndex => SceneManager.GetActiveScene().buildIndex;

    /// <summary>If Unity scene has been loaded. Does not mean that a saved state is loaded or that a client is synced with host.</summary>
    public bool IsSceneLoaded
    {
        get
        {
            return _isSceneLoaded;
        }
        private set
        {
            _isSceneLoaded = value;
            if (value == true)
            {
                if (NetworkManager._instance.ShouldSendToServer)
                {
                    // Client can send a sync request once the scene is loaded
                    ClientSend.SyncRequest();
                }
            }
            else
            {
                if (NetworkManager._instance.ShouldSendToClient)
                {
                    // Every client's scene is considered to not be loaded
                    Server.Instance.ResetConnectionFlags(ConnectionState.SceneLoaded);
                }
            }
        }
    }

    /// <summary>Gamestate is fully loaded.</summary>
    public bool IsFullyLoaded
    {
        get
        {
            return _isFullyLoaded;
        }
        private set
        {
            _isFullyLoaded = value;
            if (value == true)
            {
                if (NetworkManager._instance.ShouldSendToClient)
                {
                    // Host can send sync messages to all available clients now
                    SyncAllObjects();
                }
            }
            else
            {
                if (NetworkManager._instance.ShouldSendToClient)
                {
                    // Every client is now considered to be unsynced
                    Server.Instance.ResetConnectionFlags(ConnectionState.Synced);
                }
            }
        }
    }

    /// <summary>If client, syncing with host. If host, loading state from saved state.</summary>
    public bool IsLoadingState => IsSceneLoaded && !IsFullyLoaded;

    /// <summary>If in process of or going to load a save file.</summary>
    public bool WillLoadSave { get; private set; } = false;

    // Private values
    private bool _isSceneLoaded = false;
    private bool _isFullyLoaded = false;

    // Scene indexes
    [SerializeField]
    private int _mainMenu;
    [SerializeField]
    private int[] _levels;

    public string GetName()
    {
        // Try to get cached name first
        if (Name != null)
        {
            return Name;
        }

        // Try to retrieve from playerPrefs second
        if (PlayerPrefs.HasKey("Name"))
        {
            return PlayerPrefs.GetString("Name");
        }

        // No name saved
        return null;
    }

    public void SetName(string name)
    {
        // Makes sure that empty strings are not saved
        if (string.IsNullOrEmpty(name)) return;
        
        // Save and cache
        PlayerPrefs.SetString("Name", name);
        Name = name;
    }

    public void StartLoading(bool willLoadSave = false)
    {
        DontDestroyCanvas.Instance.loadingScreen.SetActive(true);

        Time.timeScale = 0;

        // Set everything to "not loaded" initially
        IsFullyLoaded = false;
        IsSceneLoaded = false;

        if (willLoadSave)
        {
            WillLoadSave = true;
        }

        if (NetworkManager._instance.ShouldSendToClient)
        {
            ServerSend.StartLoading();
        }
    }

    public void EndLoading()
    {
        Time.timeScale = 1;

        IsFullyLoaded = true;
        WillLoadSave = false;

        DontDestroyCanvas.Instance.loadingScreen.SetActive(false);
    }

    public void LoadNextLevel()
    {
        int nextLevel = CurrentSceneIndex + 1;
        if (nextLevel - _levels[0] < _levels.Length)
        {
            LoadLevel(nextLevel);
        }
        else
        {
            ExitToMainMenu();
        }
    }

    public void ExitToMainMenu()
    {
        NetworkManager._instance.ResetNetworking();
        if (CurrentSceneIndex != _mainMenu)
        {
            LoadLevel(_mainMenu);
        }
    }

    public void LoadLevel(int sceneIndex)
    {
        StartLoading();

        LoadScene(sceneIndex, true);

        StartCoroutine(WaitForSceneLoad(() => EndLoading()));
    }

    private IEnumerator WaitForSceneLoad(Action action)
    {
        while (!IsSceneLoaded)
        {
            yield return null;
        }

        action();
    }

    public void NewGame()
    {
        LoadLevel(_levels[0]);
    }

    public void LoadScene(int sceneIndex, bool restart = true)
    {
        if (NetworkManager._instance.ShouldSendToClient)
        {
            ServerSend.LoadScene(sceneIndex);
        }

        if (restart || CurrentSceneIndex != sceneIndex)
        {
            // We must reset all object collections but we don't need to destroy them
            // Objects are instead destroyed implictly
            ClearAllObjects(true);  // Just to be sure
            StartCoroutine(LoadSceneAsynchronously(sceneIndex));
        }
        else
        {
            // We must destroy all objects that are non-static
            ClearAllObjects();
            IsSceneLoaded = true;
        }
    }

    

    private IEnumerator LoadSceneAsynchronously(int sceneIndex, Save state = null)
    {
        AsyncOperation loadSceneOperation = SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Single);

        // TODO: Update loading screen text to "Loading scene"

        while (!loadSceneOperation.isDone)
        {
            float progress = Mathf.Clamp01(loadSceneOperation.progress / Constants.maxSceneLoadProgress);

            DontDestroyCanvas.Instance.loadingScreenSlider.value = progress;

            yield return null;
        }

        IsSceneLoaded = true;
    }

    public void StartSaving()
    {

    }

    public void EndSaving()
    {

    }

    private Save CreateSaveObject()
    {
        Save save = new Save();

        // Add objects
        foreach (Dictionary<int, ObjectManager> list in _objectLists.Values)
        {
            foreach (ObjectManager objectManager in list.Values)
            {
                save.Objects.Add(new Save.SavedObject(objectManager));
            }
        }

        // Add counters
        foreach (KeyValuePair<ObjectList, int> pair in _counters)
        {
            save.Counters.Add(pair.Key, pair.Value);
        }

        // Add scene index
        save.SceneIndex = CurrentSceneIndex;

        return save;
    }

    public void SaveToFile(string saveName = "quicksave")
    {
        Save save = CreateSaveObject();

        // Create file
        BinaryFormatter bf = new BinaryFormatter();
        string filePath = Application.persistentDataPath + "/" + saveName + ".save";
        FileStream file = File.Create(filePath);
        bf.Serialize(file, save);
        file.Close();

        // Set date
        FileInfo fileInfo = new FileInfo(filePath);
        fileInfo.CreationTime = DateTime.Now;

        MessageLog.Instance.AddMessage("Game saved", Color.green);
    }

    public bool LoadFromFile(string saveName = "quicksave.save")
    {
        string filePath = Application.persistentDataPath + "/" + saveName;
        if (!DoesFileExist(filePath)) return false;

        // Initialize loading
        StartLoading(true);

        // Get save object
        // TODO: Add error handling
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(filePath, FileMode.Open);
        Save save = (Save)bf.Deserialize(file);
        file.Close();

        // Load scene
        LoadScene(save.SceneIndex);

        // Load game state
        StartCoroutine(WaitForSceneLoad(() => LoadGameState(save)));

        return true;
    }

    private bool DoesFileExist(string filePath)
    {
        if (!File.Exists(filePath))
        {
            MessageLog.Instance.AddMessage($"Save file does not exist", Color.red);
            return false;
        }

        return true;
    }

    public List<SaveUIObject> GetSaveFiles()
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(Application.persistentDataPath + "/");
        FileInfo[] fileInfos = directoryInfo.GetFiles();

        List<SaveUIObject> saves = new List<SaveUIObject>();

        for (int i = 0; i < fileInfos.Length; ++i)
        {
            FileInfo fileInfo = fileInfos[i];
            string extension = fileInfo.Extension;

            // Is save file
            if (extension != ".save") continue;

            SaveUIObject save = new SaveUIObject();
            save.Name = fileInfo.Name;
            save.CreationDate = fileInfo.CreationTime;
            saves.Add(save);
        }

        return saves;
    }

    public bool DeleteFile(string saveName)
    {
        string filePath = Application.persistentDataPath + "/" + saveName;
        if (!DoesFileExist(filePath)) return false;

        File.Delete(filePath);

        return true;
    }

    private void LoadGameState(Save save)
    {
        Debug.Log("Loading gamestate");
        // This is just a safety precaution, as objects should be destroyed during scene loading
        ClearAllObjects();

        // Set counters
        foreach (KeyValuePair<ObjectList, int> pair in save.Counters)
        {
            if (_counters.ContainsKey(pair.Key))
            {
                _counters[pair.Key] = pair.Value;
            }
            else
            {
                _counters.Add(pair.Key, pair.Value);
            }
        }

        // Instantiate
        foreach (Save.SavedObject savedObject in save.Objects)
        {
            if (!savedObject.IsStatic)
            {
                CreateObjectWithId(savedObject.Type, savedObject.Id, Vector3.zero, Quaternion.identity);
            }
        }

        // Set states
        foreach (Save.SavedObject savedObject in save.Objects)
        {
            if (TryGetObject(savedObject.List, savedObject.Id, out ObjectManager objectManager))
            {
                using (Packet dataPacket = new Packet(savedObject.Data.ToArray()))
                {
                    objectManager.ReadState(dataPacket);
                }
            }
        }

        EndLoading();
    }


    #endregion

    #region Core
    public bool TryGetObject(ObjectList list, int id, out ObjectManager netManager)
    {
        // TODO: Check if scene is loaded. Maybe not necessary even?
        if (_objectLists[list].TryGetValue(id, out netManager))
        {
            return true;
        }
        else
        {
            Debug.Log("Object was not found. Something is wrong!");
            return false;
        }
    }

    private void AddObject(int id, ObjectManager netManager)
    {
        ObjectList list = netManager.List;

        if (_objectLists[list].ContainsKey(id))
        {
            Debug.Log("Object with the id already exists!");
            return;
        }

        _objectLists[list].Add(id, netManager);
        netManager.Id = id;
    }

    private int CreateNextId(ObjectList list)
    {
        int nextAvailableId;
        if (!_counters.TryGetValue(list, out nextAvailableId))
        {
            _counters.Add(list, 0);  // Create counter
            nextAvailableId = 0;
        }

        // Increment id for next creation
        _counters[list]++;

        return nextAvailableId;
    }
    #endregion

    #region Syncing/Clearing
    public void ResetAll()
    {
        // Objects
        foreach (KeyValuePair<ObjectList, Dictionary<int, ObjectManager>> listEntry in _objectLists)
        {
            listEntry.Value.Clear();
        }

        // Controlled characters
        ResetControlledCharacters();

        // Counters
        _counters.Clear();

        // Static object references
        Priest = null;
        Pharaoh = null;
    }

    public void ClearAllObjects(bool forceDestroy = false)
    {
        // Objects
        foreach (KeyValuePair<ObjectList, Dictionary<int, ObjectManager>> listEntry in _objectLists)
        {
            foreach (KeyValuePair<int, ObjectManager> objectEntry in listEntry.Value.ToList())
            {
                ObjectManager netManager = objectEntry.Value;
                if (netManager.Delete(forceDestroy)) listEntry.Value.Remove(objectEntry.Key);
            }
        }

        if (forceDestroy)
        {
            ResetAll();
        }
        else
        {
            // Controlled characters
            ResetControlledCharacters();

            // Counters
            _counters.Clear();
        }
    }

    public void ResetControlledCharacters()
    {
        foreach (int id in Players.Keys)
        {
            Players[id].ControlledCharacter = null;
        }
    }

    public void SyncAllObjects()
    {
        // Do not sync if scene is not loaded
        if (!IsFullyLoaded) return;

        // First create objects
        foreach (KeyValuePair<ObjectList, Dictionary<int, ObjectManager>> listEntry in _objectLists)
        {
            foreach (KeyValuePair<int, ObjectManager> objectEntry in listEntry.Value)
            {
                ObjectManager objectManager = objectEntry.Value;
                objectManager.ObjectCreated(true);
            }
        }

        // Then sync them all
        foreach (KeyValuePair<ObjectList, Dictionary<int, ObjectManager>> listEntry in _objectLists)
        {
            foreach (KeyValuePair<int, ObjectManager> objectEntry in listEntry.Value)
            {
                ObjectManager objectManager = objectEntry.Value;
                objectManager.SyncObject();
            }
        }

        // After syncing the client has loaded
        ServerSend.EndLoading();

        // All of the clients whose level was loaded should now be synced
        // This means we can start sending packets that require syncing to be complete
        Server.Instance.SetConnectionFlags(ConnectionState.Synced,
            ConnectionState.SceneLoaded);
    }
    #endregion

    #region Creators/Adders

    public void ObjectCreatedHost(ObjectManager netManager, int? specificId = null)
    {
        // TODO: Update index even when adding preindexed objects

        int id = specificId.HasValue ? specificId.Value : CreateNextId(netManager.List);

        AddObject(id, netManager);

        netManager.ObjectCreated();
    }
    public GameObject InstantiateObject(ObjectType type, Vector3 position, Quaternion rotation)
    {
        return Instantiate(_objectPrefabs[type], position, rotation);
    }

    public void CreateObjectWithId(ObjectType type, int id, Vector3 position, Quaternion rotation)
    {
        GameObject newObject = InstantiateObject(type, position, rotation);
        AddObject(id, newObject.GetComponent<ObjectManager>());
    }
    #endregion

    #region Players
    public GameObject Priest { get; private set; } = null;
    public GameObject Pharaoh { get; private set; } = null;

    public int CurrentPlayerId { get; set; } = Constants.defaultConnectionId;
    public Dictionary<int, PlayerInfo> Players { get; private set; } 
        = new Dictionary<int, PlayerInfo>();
    public ObjectType? ControlledCharacter
        => Players[CurrentPlayerId].ControlledCharacter;
    public GameObject CurrentCharacter
    {
        get
        {
            switch (ControlledCharacter)
            {
                case ObjectType.pharaoh:
                    return Pharaoh;
                case ObjectType.priest:
                    return Priest;
            }
            return null;
        }
    }

    public void AddPlayerCharacter(ObjectManager netManager)
    {
        switch (netManager.Type)
        {
            case ObjectType.pharaoh:
                Pharaoh = netManager.gameObject;
                break;
            case ObjectType.priest:
                Priest = netManager.gameObject;
                break;
        }
    }

    public void PlayerConnected(int id, string name)
    {
        Players.Add(id, new PlayerInfo
        {
            Name = name,
            ControlledCharacter = null
        });

        MessageLog.Instance.AddMessage($"{name} connected", Constants.messageColorNetworking);

        if (NetworkManager._instance.ShouldSendToClient)
        {
            ServerSend.PlayerConnected(id, name);
        }
    }

    public void PlayerDisconnected(int id)
    {
        Debug.Log($"{Players[id].Name} disconnected");

        MessageLog.Instance.AddMessage($"{Players[id].Name} disconnected", Constants.messageColorNetworking);

        if (NetworkManager._instance.IsHost)
        {
            // Unselect the character of the disconnecting player
            UnselectCharacter(id);

            if (NetworkManager._instance.ShouldSendToClient)
            {
                ServerSend.PlayerDisconnected(id);
            }
        }

        Players.Remove(id);
    }

    public void UnselectCharacter(int controllerId = Constants.defaultConnectionId)
    {
        ObjectType? character = Players[controllerId].ControlledCharacter;

        // Make sure there is character to unselect
        if (!character.HasValue) return;

        // Unselect character
        if (TryGetObject(ObjectList.player, (int)character.Value, out ObjectManager objectManager))
        {
            PlayerObjectManager playerObjectManager = (PlayerObjectManager)objectManager;
            if (playerObjectManager.Controller == controllerId)
            {
                playerObjectManager.Controller = null;

                if (NetworkManager._instance.ShouldSendToClient)
                {
                    ServerSend.CharacterControllerUpdate(character.Value, Constants.noId);
                }
            }
        }
    }

    public void SelectCharacter(ObjectType character, int controllerId = Constants.defaultConnectionId)
    {
        if (TryGetObject(ObjectList.player, (int)character, out ObjectManager objectManager))
        {
            PlayerObjectManager playerObjectManager = (PlayerObjectManager)objectManager;
            if (playerObjectManager.Controller == null)
            {
                // Unselect before selecting
                UnselectCharacter(controllerId);

                playerObjectManager.Controller = controllerId;

                if (NetworkManager._instance.ShouldSendToClient)
                {
                    ServerSend.CharacterControllerUpdate(character, controllerId);
                }
            }
        }
    }

    #endregion
}
