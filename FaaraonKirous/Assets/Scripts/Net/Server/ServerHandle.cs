using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerHandle
{
    #region Core
    public static void ConnectionRequest(int connection, Packet packet)
    {
        Debug.Log($"{GameManager._instance.Players.Count} Join request received!");

        string name = packet.ReadString();
        string password = packet.ReadString();

        // Check password
        if (!password.Equals(Server.Instance.Password))
        {
            Server.Instance.DisconnectClient(connection);
        }

        Server.Instance.SetConnectionFlags(connection, ConnectionState.Connected);
        GameManager._instance.PlayerConnected(connection, name);
        
        // Send connection accepted message
        ServerSend.ConnectionAccepted(connection);

        // Sync player info
        ServerSend.SyncPlayers(connection);

        // Start loading
        ServerSend.StartLoading(connection);

        // Send load scene order if scene is fully loaded
        if (GameManager._instance.IsFullyLoaded)
        {
            ServerSend.LoadScene(GameManager._instance.CurrentSceneIndex, connection);
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

    public static void Disconnecting(int connection, Packet packet)
    {
        Server.Instance.DisconnectClient(connection);
    }
    #endregion

    #region LoadAndSave
    public static void SyncRequest(int connection, Packet packet)
    {
        // Clients level has loaded and is ready to sync
        Server.Instance.SetConnectionFlags(connection, ConnectionState.SceneLoaded);

        // Sync if possible
        GameManager._instance.SyncAllObjects();
    }
    #endregion

    #region Abilities
    public static void AbilityUsed(int connection, Packet packet)
    {
        if (!Server.Instance.IsSynced(connection)) return;

        AbilityOption ability = (AbilityOption)packet.ReadByte();
        Vector3 position = packet.ReadVector3();

        AbilitySpawner.Instance.SpawnAtPosition(position, ability);

        ServerSend.AbilityVisualEffectCreated(connection, ability, position);
    }

    public static void EnemyPossessed(int connection, Packet packet)
    {
        if (!Server.Instance.IsSynced(connection)) return;

        int id = packet.ReadInt();
        Vector3 position = packet.ReadVector3();

        if (GameManager._instance.TryGetObject(ObjectList.enemy, id, out ObjectManager netManager))
        {
            EnemyObjectManager enemyNetManager = (EnemyObjectManager)netManager;
            enemyNetManager.Character.PossessAI(position);
        }
    }
    #endregion

    #region Player
    public static void SelectCharacterRequest(int connection, Packet packet)
    {
        if (!Server.Instance.IsSynced(connection)) return;

        ObjectType character = (ObjectType)packet.ReadShort();

        GameManager._instance.SelectCharacter(character, connection);
    }

    public static void UnselectCharacterRequest(int connection, Packet packet)
    {
        if (!Server.Instance.IsSynced(connection)) return;

        GameManager._instance.UnselectCharacter(connection);
    }

    public static void SetDestinationRequest(int connection, Packet packet)
    {
        if (!Server.Instance.IsSynced(connection)) return;

        ObjectType character = (ObjectType)packet.ReadShort();
        Vector3 destination = packet.ReadVector3();

        if (GameManager._instance.TryGetObject(ObjectList.player, (int)character, out ObjectManager netManager))
        {
            
            PlayerObjectManager playerNetManager = (PlayerObjectManager)netManager;
            playerNetManager.PlayerController.SetDestination(destination);
        }
    }

    public static void KillEnemy(int connection, Packet packet)
    {
        if (!Server.Instance.IsSynced(connection)) return;

        int id = packet.ReadInt();

        if (GameManager._instance.TryGetObject(ObjectList.enemy, id, out ObjectManager netManager))
        {
            EnemyObjectManager enemyNetManager = (EnemyObjectManager)netManager;
            enemyNetManager.DeathScript.Die();
        }
    }

    public static void Revive(int connection, Packet packet)
    {
        if (!Server.Instance.IsSynced(connection)) return;

        int id = packet.ReadInt();

        if (GameManager._instance.TryGetObject(ObjectList.player, id, out ObjectManager objectManager))
        {
            PlayerObjectManager playerObjectManager = (PlayerObjectManager)objectManager;
            playerObjectManager.DeathScript.Revive();
        }
    }

    public static void Crouching(int connection, Packet packet)
    {
        if (!Server.Instance.IsSynced(connection)) return;

        ObjectType character = (ObjectType)packet.ReadShort();
        bool state = packet.ReadBool();

        if (GameManager._instance.TryGetObject(ObjectList.player, (int)character, out ObjectManager netManager))
        {
            PlayerObjectManager playerNetManager = (PlayerObjectManager)netManager;
            playerNetManager.PlayerController.IsCrouching = state;

            ServerSend.Crouching(character, state, connection);
        }
    }

    public static void Running(int connection, Packet packet)
    {
        if (!Server.Instance.IsSynced(connection)) return;

        ObjectType character = (ObjectType)packet.ReadShort();
        bool state = packet.ReadBool();

        if (GameManager._instance.TryGetObject(ObjectList.player, (int)character, out ObjectManager netManager))
        {
            PlayerObjectManager playerNetManager = (PlayerObjectManager)netManager;
            playerNetManager.PlayerController.IsRunning = state;

            ServerSend.Running(character, state, connection);
        }
    }

    public static void Stay(int connection, Packet packet)
    {
        if (!Server.Instance.IsSynced(connection)) return;

        ObjectType character = (ObjectType)packet.ReadShort();

        if (GameManager._instance.TryGetObject(ObjectList.player, (int)character, out ObjectManager objectManager))
        {
            PlayerObjectManager playerNetManager = (PlayerObjectManager)objectManager;
            playerNetManager.PlayerController.Stay();
        }
    }

    public static void Warp(int connection, Packet packet)
    {
        if (!Server.Instance.IsSynced(connection)) return;

        ObjectType character = (ObjectType)packet.ReadShort();
        Vector3 position = packet.ReadVector3();

        if (GameManager._instance.TryGetObject(ObjectList.player, (int)character, out ObjectManager objectManager))
        {
            PlayerObjectManager playerNetManager = (PlayerObjectManager)objectManager;
            playerNetManager.PlayerController.navMeshAgent.Warp(position);
        }
    }

    #endregion

    #region Activatable

    public static void ActivateObject(int connection, Packet packet)
    {
        if (!Server.Instance.IsSynced(connection)) return;

        int id = packet.ReadInt();

        if (GameManager._instance.TryGetObject(ObjectList.activatable, id, out ObjectManager objectManager))
        {
            ActivatableObjectManager activatableObjectManager = (ActivatableObjectManager)objectManager;
            activatableObjectManager.ActivatorScript.Activate();
        }
    }


    #endregion
}
