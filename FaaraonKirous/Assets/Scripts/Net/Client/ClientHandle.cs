﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientHandle
{
    #region Core
    public static void ConnectionAccepted(int connection, Packet packet)
    {
        int sendId = packet.ReadInt();
        string msg = packet.ReadString();

        Debug.Log($"Message from server: {msg}");
        Client.Instance.Connection.SendId = sendId;
        ClientSend.ConnectionAcceptedReceived();
    }
    public static void Message(int connection, Packet packet)
    {
        string msg = packet.ReadString();

        Debug.Log($"Message from server: {msg}");
    }

    public static void Heartbeat(int connection, Packet packet)
    {
        long timeStamp = packet.ReadLong();
        int lastPing = packet.ReadInt();

        Client.Instance.Connection.Ping = lastPing;

        ClientSend.HeartbeatReceived(timeStamp);
    }
    #endregion

    #region ObjectSyncing
    public static void StartingObjectSync(int connection, Packet packet)
    {
        // TODO: Pause game here too

        GameManager._instance.ClearAllObjects();
    }

    public static void SyncObject(int connection, Packet packet)
    {
        ObjectList list = (ObjectList)packet.ReadByte();
        int id = packet.ReadInt();

        if (GameManager._instance.TryGetObject(list, id, out ObjectNetManager netManager))
        {
            netManager.HandleSync(packet);
        }
        else
        {
            Debug.Log("Object not in gameManager! Something is wrong.");
        }
    }
    #endregion

    #region ObjectCreation
    public static void ObjectCreated(int connection, Packet packet)
    {
        ObjectType type = (ObjectType)packet.ReadShort();
        int id = packet.ReadInt();
        Vector3 position = packet.ReadVector3();
        Quaternion rotation = packet.ReadQuaternion();

        GameManager._instance.CreateObjectClient(type, id, position, rotation);
    }

    public static void DisposableObjectCreated(int connection, Packet packet)
    {
        ObjectType type = (ObjectType)packet.ReadShort();
        Vector3 position = packet.ReadVector3();
        Quaternion rotation = packet.ReadQuaternion();

        GameManager._instance.InstantiateObjectClient(type, position, rotation);
    }
    #endregion

    #region ObjectUpdating
    public static void UpdateObjectTransform(int connection, Packet packet)
    {
        ObjectList list = (ObjectList)packet.ReadByte();
        int id = packet.ReadInt();
        Vector3 position = packet.ReadVector3();
        Quaternion rotation = packet.ReadQuaternion();

        if (GameManager._instance.TryGetObject(list, id, out ObjectNetManager netManager))
        {
            netManager.UpdateTransform(position, rotation);
        }
    }
    #endregion

}

