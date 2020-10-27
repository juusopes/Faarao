using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerHandle
{
    public static void ConnectionRequest(int connection, Packet packet)
    {
        ServerSend.ConnectionAccepted(connection);
    }

    public static void ConnectionAcceptedReceived(int connection, Packet packet)
    {
        string msg = packet.ReadString();

        Debug.Log($"Connection acceptance received. Contains message: {msg}");

        // For testing
        //for (int i = 1; i <= 100; ++i)
        //{
        //    ServerSend.Message(connection, i.ToString());
        //}

        // TODO: Pause game here. And wait that all clients are paused too

        // TODO: This should propably be done by the network manager
        GameManager._instance.SyncAllObjects();
    }

    public static void HeartbeatReceived(int connection, Packet packet)
    {
        long timeStamp = packet.ReadLong();
        // TODO: The ping is fundamentally dependant on the fixedTime frequency, and the speed of handling other requests
        // TODO: int overflow might occur here
        int ping = (int)((DateTime.Now.Ticks - timeStamp) / TimeSpan.TicksPerMillisecond);
        // TODO: Check that connection is not null
        Server.Instance.Connections[connection].Ping = ping;
    }

    #region Abilities

    public static void AbilityUsed(int connection, Packet packet)
    {
        AbilityOption ability = (AbilityOption)packet.ReadByte();
        Vector3 position = packet.ReadVector3();

        AbilitySpawner.Instance.SpawnAtPosition(position, ability);

        ServerSend.AbilityVisualEffectCreated(connection, ability, position);
    }

    public static void EnemyPossessed(int connection, Packet packet)
    {
        int id = packet.ReadInt();
        Vector3 position = packet.ReadVector3();

        if (GameManager._instance.TryGetObject(ObjectList.enemy, id, out ObjectNetManager netManager))
        {
            EnemyNetManager enemyNetManager = (EnemyNetManager)netManager;
            enemyNetManager.Character.PossessAI(position);
        }
    }
    #endregion
}
