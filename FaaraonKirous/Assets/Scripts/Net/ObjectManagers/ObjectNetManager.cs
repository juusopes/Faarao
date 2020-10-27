using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectNetManager : MonoBehaviour
{
    public int Id { get; set; }
    public ObjectList List { get { return _list; } private set { _list = value; } }
    public ObjectType Type { get { return _type; } private set { _type = value; } }
    public bool ShouldSendServer { get { return NetworkManager._instance.IsHost && Server.Instance.IsOnline; } }
    public bool ShouldSendClient { get { return !NetworkManager._instance.IsHost && NetworkManager._instance.IsConnectedToServer; } }
    public Transform Transform { get; private set; }

    [SerializeField]
    private ObjectList _list;
    [SerializeField]
    private ObjectType _type;

    protected virtual void Awake()
    {
        Transform = transform;
    }

    protected virtual void Start()
    {
        if (NetworkManager._instance.IsHost)
        {
            GameManager._instance.ObjectCreatedHost(this);
        } 
        else
        {
            if (!NetworkManager._instance.IsConnectedToServer) Destroy(gameObject);
        }
    }

    protected virtual void Update()
    {
        // ...
    }

    protected virtual void FixedUpdate()
    {
        if (ShouldSendServer) ServerSend.UpdateObjectTransform(List, Id, Transform.position, Transform.rotation);
    }

    public virtual void SendSync(Packet packet)
    {
        // ...
    }

    public virtual void HandleSync(Packet packet)
    {
        // ...
    }

    public void UpdateTransform(Vector3 position, Quaternion quaternion)
    {
        Transform.position = position;
        Transform.rotation = quaternion;
    }

    public void Delete()
    {
        Destroy(gameObject);
    }
}
