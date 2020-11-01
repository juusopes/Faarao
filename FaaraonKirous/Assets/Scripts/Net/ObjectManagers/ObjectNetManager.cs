using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ObjectNetManager : MonoBehaviour
{
    public int Id { get; set; }
    public ObjectList List { get { return _list; } private set { _list = value; } }
    public ObjectType Type { get { return _type; } private set { _type = value; } }
    public Transform Transform { get; private set; }

    public bool IsStatic { get; protected set; } = false;
    public bool IsSyncable { get; protected set; } = true;

    [SerializeField]
    private ObjectList _list;
    [SerializeField]
    private ObjectType _type;

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
        if (IsStatic)
        {
            GameManager._instance.ObjectCreatedHost(this, true);
        }
        else
        {
            if (NetworkManager._instance.IsHost)
            {
                GameManager._instance.ObjectCreatedHost(this);
            }
            else if (!NetworkManager._instance.IsConnectedToServer)
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
        // ...
    }

    public virtual void HandleSync(Packet packet)
    {
        // ...
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

    public bool ObjectCreated()
    {
        if (IsStatic) return false;

        if (NetworkManager._instance.ShouldSendToClient)
        {
            ServerSend.ObjectCreated(Type, Id, Transform.position, Transform.rotation);
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
