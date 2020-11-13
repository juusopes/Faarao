using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
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

    private readonly Dictionary<ObjectList, Dictionary<int, ObjectNetManager>> _objectLists = new Dictionary<ObjectList, Dictionary<int, ObjectNetManager>>();
    private readonly Dictionary<ObjectList, int> _counters = new Dictionary<ObjectList, int>();

    // Scene
    public int CurrentSceneIndex => SceneManager.GetActiveScene().buildIndex;
    public bool IsSceneLoaded { get; private set; } = false;
    public bool IsLoading { get; set; } = false;

    // Prefabs
    private readonly Dictionary<ObjectType, GameObject> _objectPrefabs = new Dictionary<ObjectType, GameObject>();
    [SerializeField]
    private GameObject _enemyClientPrefab = null;

    // Player Characters
    public GameObject Priest { get; private set; } = null;
    public GameObject Pharaoh { get; private set; } = null;

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

        // Add lists
        _objectLists.Add(ObjectList.enemy, new Dictionary<int, ObjectNetManager>());
        _objectLists.Add(ObjectList.player, new Dictionary<int, ObjectNetManager>());
        _objectPrefabs.Add(ObjectType.enemy, _enemyClientPrefab);

        // For testing
        if (NetworkManager._instance.Testing && NetworkManager._instance.IsHost)
        {
            IsSceneLoaded = true;
        }
    }

    #region Loading
    public void StartLoading()
    {
        Time.timeScale = 0;

        IsLoading = true;
        IsSceneLoaded = false;

        // TODO: Start loading screen

        if (NetworkManager._instance.ShouldSendToClient)
        {
            Server.Instance.ResetConnectionFlags(ConnectionState.LevelLoaded
                | ConnectionState.Synced);
        }

        ClearAllObjects();
    }

    public void LoadSave(string saveFile)
    {
        StartLoading();

        // TODO: Get save file

        // TODO: If scene is different load it.

        // TODO: Else just load save
    }

    public void LoadScene(int index, bool restartScene = false)
    {
        StartLoading();

        if (NetworkManager._instance.ShouldSendToClient)
        {
            ServerSend.LoadScene(index);
        }

        if (restartScene || CurrentSceneIndex != index)
        {
            Debug.Log("Scene not loaded or restarting. Loading scene.");

            // Reset everything if level is changed
            ResetAll();

            SceneManager.sceneLoaded += OnSceneLoad;
            SceneManager.LoadSceneAsync(index, LoadSceneMode.Single);
        }
        else
        {
            Debug.Log("Scene already loaded. Don't need to load.");

            // Scene is loaded if it is the current scene
            SceneLoaded();
        }
    }
    
    public void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoad;
        SceneLoaded();
    }

    public void SceneLoaded()
    {
        Debug.Log("Scene loaded");
        IsSceneLoaded = true;

        if (NetworkManager._instance.IsHost)
        {
            if (NetworkManager._instance.ShouldSendToClient)
            {
                // This makes sure that clients who are waiting for syncing can sync
                SyncAllObjects();
            }

            EndLoading();
        }
        else
        {
            if (NetworkManager._instance.ShouldSendToServer)
            {
                Debug.Log("Scene loaded: Sending sync request");
                ClientSend.SyncRequest();
            }
        }
    }

    public void EndLoading()
    {
        // TODO: Exit loading screen

        IsLoading = false;

        Time.timeScale = 1;
    }
    #endregion

    #region Core
    public bool TryGetObject(ObjectList list, int id, out ObjectNetManager netManager)
    {
        // TODO: Check if scene is loaded. Maybe not necessary even?
        return _objectLists[list].TryGetValue(id, out netManager);
    }

    private void AddObject(int id, ObjectNetManager netManager)
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
        if (!_counters.TryGetValue(list, out nextAvailableId)) {
            _counters.Add(list, 0);  // Create counter
            nextAvailableId = 0;
        }

        // Increment id for next creation
        _counters[list]++;

        return nextAvailableId;
    }
    #endregion

    #region Syncing
    public void ResetAll()
    {
        // Object lists
        foreach (KeyValuePair<ObjectList, Dictionary<int, ObjectNetManager>> listEntry in _objectLists)
        {
            listEntry.Value.Clear();
        }

        // Counters
        _counters.Clear();

        // Static object references
        Priest = null;
        Pharaoh = null;
    }

    public void ClearAllObjects()
    {
        foreach (KeyValuePair<ObjectList, Dictionary<int, ObjectNetManager>> listEntry in _objectLists)
        {
            foreach (KeyValuePair<int, ObjectNetManager> objectEntry in listEntry.Value.ToList())
            {
                ObjectNetManager netManager = objectEntry.Value;
                if (netManager.Delete()) listEntry.Value.Remove(objectEntry.Key);
            }
        }

        _counters.Clear();
    }

    public void SyncAllObjects()
    {
        // Do not sync if scene is not loaded
        if (!IsSceneLoaded) return;

        ServerSend.StartObjectSync();

        // First create objects
        foreach (KeyValuePair<ObjectList, Dictionary<int, ObjectNetManager>> listEntry in _objectLists)
        {
            foreach (KeyValuePair<int, ObjectNetManager> objectEntry in listEntry.Value)
            {
                ObjectNetManager netManager = objectEntry.Value;
                netManager.ObjectCreated(true);
            }
        }

        // Then sync them all
        foreach (KeyValuePair<ObjectList, Dictionary<int, ObjectNetManager>> listEntry in _objectLists)
        {
            foreach (KeyValuePair<int, ObjectNetManager> objectEntry in listEntry.Value)
            {
                ObjectNetManager netManager = objectEntry.Value;
                netManager.SyncObject();
            }
        }

        // After syncing the client has loaded
        ServerSend.EndLoading();

        // All of the clients whose level was loaded should now be synced
        // This means we can start sending packets that require syncing to be complete
        Server.Instance.SetConnectionFlags(ConnectionState.Synced,
            ConnectionState.LevelLoaded,
            ConnectionState.Synced);
    }
    #endregion

    #region Adders
    public void AddPlayerCharacter(ObjectNetManager netManager)
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

    public void ObjectCreatedHost(ObjectNetManager netManager, bool useTypeForId = false)
    {
        ObjectList list = netManager.List;

        int id = useTypeForId ? (int)netManager.Type : CreateNextId(list);
        AddObject(id, netManager);

        netManager.ObjectCreated();
    }
    #endregion

    #region Creators
    public GameObject InstantiateObjectClient(ObjectType type, Vector3 position, Quaternion rotation)
    {
        return Instantiate(_objectPrefabs[type], position, rotation);
    }

    public void CreateObjectClient(ObjectType type, int id, Vector3 position, Quaternion rotation)
    {
        GameObject newObject = InstantiateObjectClient(type, position, rotation);
        AddObject(id, newObject.GetComponent<ObjectNetManager>());
    }
    #endregion
}
