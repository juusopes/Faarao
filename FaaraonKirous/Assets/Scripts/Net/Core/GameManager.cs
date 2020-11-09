using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

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
        }

        // Add lists
        _objectLists.Add(ObjectList.enemy, new Dictionary<int, ObjectNetManager>());
        _objectLists.Add(ObjectList.player, new Dictionary<int, ObjectNetManager>());
        _objectPrefabs.Add(ObjectType.enemy, _enemyClientPrefab);
    }

    #region Core
    public bool TryGetObject(ObjectList list, int id, out ObjectNetManager netManager)
    {
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
    public void ClearAllObjects()
    {
        foreach (KeyValuePair<ObjectList, Dictionary<int, ObjectNetManager>> listEntry in _objectLists)
        {
            foreach (KeyValuePair<int, ObjectNetManager> objectEntry in listEntry.Value)
            {
                ObjectNetManager netManager = objectEntry.Value;
                if (netManager.Delete()) listEntry.Value.Remove(objectEntry.Key);
            }
        }

        // TODO: Reset counters. Though we don't care about them as client should not create objects
    }

    public void SyncAllObjects()
    {
        ServerSend.StartingObjectSync();

        // First create objects
        foreach (KeyValuePair<ObjectList, Dictionary<int, ObjectNetManager>> listEntry in _objectLists)
        {
            foreach (KeyValuePair<int, ObjectNetManager> objectEntry in listEntry.Value)
            {
                ObjectNetManager netManager = objectEntry.Value;
                netManager.ObjectCreated();
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

    #region Helpers
    public GameObject GetPlayerCharacter(ObjectType type)
    {
        if (TryGetObject(ObjectList.player, (int)type, out ObjectNetManager netManager))
        {
            return netManager.gameObject;
        }
        else
        {
            return null;
        }
    }
    #endregion



}
