using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerHandle
{
    #region Core
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

        Server.Instance.SetConnectionFlags(connection, ConnectionState.Connected);

        Debug.Log("Sending load scene request");

        if (GameManager._instance.IsSceneLoaded)
        {
            ServerSend.LoadScene(GameManager._instance.CurrentSceneIndex);
        }
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
    #endregion

    #region Load
    public static void SyncRequest(int connection, Packet packet)
    {
        // Clients level has loaded and is ready to sync
        Server.Instance.SetConnectionFlags(connection, ConnectionState.LevelLoaded);

        // Sync if possible
        GameManager._instance.SyncAllObjects();
    }
    #endregion

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

    #region

    public static void ChangeCharacterRequest(int connection, Packet packet)
    {
        ObjectType character = (ObjectType)packet.ReadShort();

        if (LevelController._instance.CanChangeToCharacter(character))
        {
            LevelController._instance.ChangeToCharacter(character, false);
            ServerSend.ChangeToCharacter(connection, character);
        }
    }

    public static void UnselectCharacter(int connection, Packet packet)
    {
        ObjectType character = (ObjectType)packet.ReadShort();

        LevelController._instance.UnselectCharacter(character);
    }

    public static void SetDestinationRequest(int connection, Packet packet)
    {
        ObjectType character = (ObjectType)packet.ReadShort();
        Vector3 destination = packet.ReadVector3();

        if (GameManager._instance.TryGetObject(ObjectList.player, (int)character, out ObjectNetManager netManager))
        {
            
            PlayerNetManager playerNetManager = (PlayerNetManager)netManager;
            playerNetManager.PlayerController.SetDestination(destination, true);
        }
    }
    #endregion
}
