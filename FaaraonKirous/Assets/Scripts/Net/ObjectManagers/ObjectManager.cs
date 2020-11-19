using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    public int Id { get { return _id; } set { _id = value; } }
    public ObjectList List { get { return _list; } private set { _list = value; } }
    public ObjectType Type { get { return _type; } private set { _type = value; } }
    public Transform Transform { get; private set; }

    public bool IsUnique { get; protected set; } = false;
    public bool IsPreIndexed { get; protected set; } = false;
    public bool IsStatic => IsUnique || IsPreIndexed;
    public bool IsSyncable { get; protected set; } = true;

    [SerializeField]
    private ObjectList _list;
    [SerializeField]
    private ObjectType _type;
    [SerializeField]
    private int _id;

    protected virtual void Awake()
    {
        InitComponents();
        AddToGameManager();
    }

    protected virtual void InitComponents()
    {
        Transform = transform;
    }

    protected virtual void AddToGameManager()
    {
        if (IsUnique)
        {
            GameManager._instance.ObjectCreatedHost(this, (int)Type);
        }
        else if (IsPreIndexed)
        {
            GameManager._instance.ObjectCreatedHost(this, Id);
        }
        else
        {
            if (NetworkManager._instance.IsHost)
            {
                // If loading from a save, the object is added to the game manager manually
                if (GameManager._instance.WillLoadSave)
                {
                    // Destroy when loading a scene as we will replace the object in any case, if loading from a save
                    if (!GameManager._instance.IsSceneLoaded)
                    {
                        Destroy(gameObject);
                    }
                }
                else
                {
                    GameManager._instance.ObjectCreatedHost(this);
                }
            }
            // Client should not create the object at all when loading a scene
            // because he receives it during the syncing process instead
            else if (!GameManager._instance.IsSceneLoaded)
            {
                Destroy(gameObject);
            }
        }
        
    }

    protected virtual void Update()
    {
        // ...
    }

    protected virtual void FixedUpdate()
    {
        // ...
    }

    public virtual void SendSync(Packet packet)
    {
        packet.Write(Transform.position);
        packet.Write(Transform.rotation);
    }

    public virtual void HandleSync(Packet packet)
    {
        ResetObject();

        Transform.position = packet.ReadVector3();
        Transform.rotation = packet.ReadQuaternion();
    }

    public virtual void ResetObject()
    {
        // ...
    }

    public virtual void WriteState(Packet dataPacket)
    {
        // We need to do basic syncing at least
        SendSync(dataPacket);
    }

    public virtual void ReadState(Packet dataPacket)
    {
        // We need to do basic syncing at least
        HandleSync(dataPacket);
    }

    public bool SyncObject()
    {
        if (!IsSyncable) return false; 

        if (NetworkManager._instance.ShouldSendToClient)
        {
            ServerSend.SyncObject(this);
        }

        return true;
    }

    public bool ObjectCreated(bool isSyncing = false)
    {
        if (IsStatic) return false;

        if (NetworkManager._instance.ShouldSendToClient)
        {
            ServerSend.ObjectCreated(Type, Id, Transform.position, Transform.rotation, isSyncing);
        }

        return true;
    }

    public bool Delete()
    {
        if (IsStatic) return false;

        Destroy(gameObject);

        return true;
    }
}
