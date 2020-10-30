using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectNetManager : MonoBehaviour
{
    public int Id { get; set; }
    public ObjectList List { get { return _list; } private set { _list = value; } }
    public ObjectType Type { get { return _type; } private set { _type = value; } }
    public Transform Transform { get; private set; }
    public long LatestTransformTimestamp { get; set; } = 0;

    public bool IsStatic { get; protected set; } = false;

    [SerializeField]
    private ObjectList _list;
    [SerializeField]
    private ObjectType _type;

    protected virtual void Awake()
    {
        Transform = transform;
        AddToGameManager();
    }

    protected virtual void AddToGameManager()
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

    protected virtual void Update()
    {
        // ...
    }

    protected virtual void FixedUpdate()
    {
        if (NetworkManager._instance.ShouldSendToClient) ServerSend.UpdateObjectTransform(List, Id, Transform.position, Transform.rotation);
    }

    public virtual void SendSync(Packet packet)
    {
        // ...
    }

    public virtual void HandleSync(Packet packet)
    {
        // ...
    }

    public void Delete()
    {
        Destroy(gameObject);
    }
}
