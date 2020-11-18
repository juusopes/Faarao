using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public enum ObjectList : byte
{
    enemy,
    player
}

public enum ObjectType : short
{
    enemy,
    pharaoh,
    priest
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
        _objectLists.Add(ObjectList.enemy, new Dictionary<int, ObjectManager>());
        _objectLists.Add(ObjectList.player, new Dictionary<int, ObjectManager>());
        _objectPrefabs.Add(ObjectType.enemy, _enemyClientPrefab);

        // For testing
        if (NetworkManager._instance.Testing && NetworkManager._instance.IsHost)
        {
            IsSceneLoaded = true;
            IsFullyLoaded = true;
        }
    }

    #region Loading/Saving
    public int CurrentSceneIndex => SceneManager.GetActiveScene().buildIndex;
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
                    ClientSend.SyncRequest();
                }
            }
            else
            {
                if (NetworkManager._instance.ShouldSendToClient)
                {
                    // Every client's scene is considered to not be loaded, if host's scene is not loaded
                    Server.Instance.ResetConnectionFlags(ConnectionState.SceneLoaded);
                }
            }
        }
    }

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
    private bool _isSceneLoaded = false;
    private bool _isFullyLoaded = false;

    public void StartLoading()
    {
        if (NetworkManager._instance.ShouldSendToClient)
        {
            ServerSend.StartLoading();
        }
        
        Time.timeScale = 0;

        IsFullyLoaded = false;
        IsSceneLoaded = false;
    }

    public void LoadLevel(int sceneIndex)
    {
        StartLoading();

        LoadScene(sceneIndex, true);

        WaitForSceneLoad(() => EndLoading());
    }

    private IEnumerator WaitForSceneLoad(Action action)
    {
        while (!IsSceneLoaded)
        {
            yield return null;
        }

        action();
    }

    public void LoadScene(int sceneIndex, bool restart = false)
    {
        if (NetworkManager._instance.ShouldSendToClient)
        {
            ServerSend.LoadScene(sceneIndex);
        }

        if (restart || CurrentSceneIndex != sceneIndex)
        {
            // We must reset all object collections but we don't need to destroy them
            // Objects are instead destroyed implictly
            ResetAll();
            StartCoroutine(LoadSceneAsynchronously(sceneIndex));
        }
        else
        {
            // We must destroy all objects that are non-static
            ClearAllObjects();
            IsSceneLoaded = true;
        }
    }

    public void EndLoading()
    {
        Time.timeScale = 1;

        IsFullyLoaded = true;
    }

    private IEnumerator LoadSceneAsynchronously(int sceneIndex, GameState state = null)
    {
        AsyncOperation loadSceneOperation = SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Single);

        // TODO: Update loading screen text to "Loading scene"

        while (!loadSceneOperation.isDone)
        {
            float progress = Mathf.Clamp01(loadSceneOperation.progress / Constants.maxSceneLoadProgress);

            // TODO: Update progress bar and percentage

            yield return null;
        }

        IsSceneLoaded = true;
        EndLoading();
    }

    public void StartSaving()
    {

    }

    public void EndSaving()
    {

    }

    public void SaveToFile(string saveName)
    {
        // TODO: 
    }

    public void LoadFromFile(string saveName)
    {
        StartLoading();

        // TODO: Deserialize saveFile
        int sceneIndex = 0;
        GameState saveState = null;

        if (CurrentSceneIndex != sceneIndex)
        {
            ResetAll();
            //StartCoroutine(LoadSceneAsynchronously(sceneIndex, saveState));
        }
        else
        {
            // StartCoroutine(LoadSceneAsynchronously(sceneIndex, saveState));
        }
    }

    private void LoadGameState(GameState state)
    {
        ClearAllObjects();
        // TODO: Instantiate objects
        // TODO: Set object states

    }


    #endregion

    #region Core
    public bool TryGetObject(ObjectList list, int id, out ObjectManager netManager)
    {
        // TODO: Check if scene is loaded. Maybe not necessary even?
        return _objectLists[list].TryGetValue(id, out netManager);
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

    public void ClearAllObjects()
    {
        // Objects
        foreach (KeyValuePair<ObjectList, Dictionary<int, ObjectManager>> listEntry in _objectLists)
        {
            foreach (KeyValuePair<int, ObjectManager> objectEntry in listEntry.Value.ToList())
            {
                ObjectManager netManager = objectEntry.Value;
                if (netManager.Delete()) listEntry.Value.Remove(objectEntry.Key);
            }
        }

        // Controlled characters
        ResetControlledCharacters();

        // Counters
        _counters.Clear();
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
                ObjectManager netManager = objectEntry.Value;
                netManager.ObjectCreated(true);
            }
        }

        // Then sync them all
        foreach (KeyValuePair<ObjectList, Dictionary<int, ObjectManager>> listEntry in _objectLists)
        {
            foreach (KeyValuePair<int, ObjectManager> objectEntry in listEntry.Value)
            {
                ObjectManager netManager = objectEntry.Value;
                netManager.SyncObject();
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

    public void ObjectCreatedHost(ObjectManager netManager, bool useTypeForId = false)
    {
        ObjectList list = netManager.List;

        int id = useTypeForId ? (int)netManager.Type : CreateNextId(list);
        AddObject(id, netManager);

        netManager.ObjectCreated();
    }
    public GameObject InstantiateObjectClient(ObjectType type, Vector3 position, Quaternion rotation)
    {
        return Instantiate(_objectPrefabs[type], position, rotation);
    }

    public void CreateObjectClient(ObjectType type, int id, Vector3 position, Quaternion rotation)
    {
        GameObject newObject = InstantiateObjectClient(type, position, rotation);
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

        if (NetworkManager._instance.ShouldSendToClient)
        {
            ServerSend.PlayerConnected(id, name);
        }
    }

    public void PlayerDisconnected(int id)
    {
        Debug.Log($"{Players[id].Name} disconnected");

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
                    ServerSend.CharacterControllerUpdate(character.Value, Constants.noConnectionId);
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
