using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientHandle
{
    #region Packets
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

    #region Enemy
    public static void EnemyCreated(int connection, Packet packet)
    {
        int enemyId = packet.ReadInt();
        Vector3 position = packet.ReadVector3();

        GameManager._instance.CreateEnemy(enemyId, position);
    }

    public static void EnemyTransform(int connection, Packet packet)
    {
        int enemyId = packet.ReadInt();
        Vector3 position = packet.ReadVector3();
        Quaternion quaternion = packet.ReadQuaternion();

        GameManager._instance.UpdateEnemyTransform(enemyId, position, quaternion);
    }
    #endregion

}

