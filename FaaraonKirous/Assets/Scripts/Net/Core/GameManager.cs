using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ObjectList : byte
{
    enemy
}

public enum ObjectType : short
{
    enemy
}

public class GameManager : MonoBehaviour
{
    public static GameManager _instance;

    private readonly Dictionary<ObjectList, Dictionary<int, ObjectNetManager>> _objectLists = new Dictionary<ObjectList, Dictionary<int, ObjectNetManager>>();
    private readonly Dictionary<int, ObjectNetManager> _enemies = new Dictionary<int, ObjectNetManager>();
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
        }

        // Add lists
        _objectLists.Add(ObjectList.enemy, _enemies);
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

    private int GetCounter(ObjectList list)
    {
        if (_counters.TryGetValue(list, out int value))
        {
            return value;
        }
        else {
            // Create counter
            _counters.Add(list, 0);
            return 0;
        }
    }

    private void IncrementCounter(ObjectList list)
    {
        if (_counters.ContainsKey(list))
        {
            _counters[list]++;
        }
        else
        {
            Debug.Log("List not found! Cannot increment.");
        }
    }
    #endregion

    #region Syncing
    public void ClearAllObjects()
    {
        foreach (KeyValuePair<ObjectList, Dictionary<int, ObjectNetManager>> listEntry in _objectLists)
        {
            foreach (KeyValuePair<int, ObjectNetManager> objectEntry in listEntry.Value)
            {
                objectEntry.Value.Delete();
            }
            // Clear dictionary ones objects have been destroyed
            listEntry.Value.Clear();
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
                ServerSend.ObjectCreated(netManager.Type, netManager.Id,
                    netManager.Transform.position, netManager.Transform.rotation);
            }
        }

        // Then sync them all
        foreach (KeyValuePair<ObjectList, Dictionary<int, ObjectNetManager>> listEntry in _objectLists)
        {
            foreach (KeyValuePair<int, ObjectNetManager> objectEntry in listEntry.Value)
            {
                ObjectNetManager netManager = objectEntry.Value;

                ServerSend.SyncObject(listEntry.Key, objectEntry.Key, netManager);
            }
        }
    }
    #endregion

    #region Adders
    public void ObjectCreatedHost(ObjectNetManager netManager)
    {
        ObjectList list = netManager.List;

        int idCounter = GetCounter(list);
        AddObject(idCounter, netManager);

        // TODO: Check if host or not
        if (Server.Instance.IsOnline)
        {
            ServerSend.ObjectCreated(netManager.Type, idCounter,
                netManager.Transform.position, netManager.Transform.rotation);
        }

        IncrementCounter(list);
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
