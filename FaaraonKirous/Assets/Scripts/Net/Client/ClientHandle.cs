using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientHandle
{
    #region MasterServer

    public static void ConnectionAcceptedMaster(int connection, Packet packet)
    {
        int sendId = packet.ReadInt();

        Client.Instance.MasterServer.SendId = sendId;
    }

    public static void HeartbeatMaster(int connection, Packet packet)
    {
        // Do nothing
    }

    public static void Handshake(int connection, Packet packet)
    {
        string endpoint = packet.ReadString();

        // TODO: Now try to connect
    }
    #endregion

    #region Core
    public static void ConnectionAccepted(int connection, Packet packet)
    {
        int sendId = packet.ReadInt();
        string msg = packet.ReadString();

        Debug.Log($"Message from server: {msg}");
        GameManager._instance.CurrentPlayerId = sendId;
        Client.Instance.Connection.SendId = sendId;
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

    public static void SyncPlayers(int connection, Packet packet)
    {
        GameManager._instance.Players.Clear();

        int count = packet.ReadInt();
        for (int i = 0; i < count; ++i)
        {
            GameManager._instance.Players.Add(packet.ReadInt(), new PlayerInfo
            {
                Name = packet.ReadString(),
                ControlledCharacter = null
            });
        }
    }

    public static void PlayerConnected(int connection, Packet packet)
    {
        int playerId = packet.ReadInt();
        string playerName = packet.ReadString();

        GameManager._instance.PlayerConnected(playerId, playerName);
    }

    public static void PlayerDisconnected(int connection, Packet packet)
    {
        int playerId = packet.ReadInt();

        GameManager._instance.PlayerDisconnected(playerId);
    }

    public static void ServerStopped(int connection, Packet packet)
    {
        Client.Instance.Disconnect();
        GameManager._instance.ExitToMainMenu();
        MessageLog.Instance.AddMessage("Server connection lost", Constants.messageColorNetworking);
    }
    #endregion

    #region LoadAndSave
    public static void StartLoading(int connection, Packet packet)
    {
        Debug.Log("Start loading message received");

        GameManager._instance.StartLoading();
    }

    public static void LoadScene(int connection, Packet packet)
    {
        int sceneIndex = packet.ReadInt();

        Debug.Log("Load scene message received");

        GameManager._instance.LoadScene(sceneIndex);
    }

    public static void EndLoading(int connection, Packet packet)
    {
        Debug.Log("End loading message received");

        GameManager._instance.EndLoading();
    }
    #endregion

    #region ObjectSyncing
    public static void SyncObject(int connection, Packet packet)
    {
        ObjectList list = (ObjectList)packet.ReadByte();
        int id = packet.ReadInt();

        if (GameManager._instance.TryGetObject(list, id, out ObjectManager netManager))
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

        GameManager._instance.CreateObjectWithId(type, id, position, rotation);
    }

    public static void DisposableObjectCreated(int connection, Packet packet)
    {
        ObjectType type = (ObjectType)packet.ReadShort();
        Vector3 position = packet.ReadVector3();
        Quaternion rotation = packet.ReadQuaternion();

        GameManager._instance.InstantiateObject(type, position, rotation);
    }
    #endregion

    #region ObjectUpdating
    public static void UpdateObjectTransform(int connection, Packet packet)
    {
        ObjectList list = (ObjectList)packet.ReadByte();
        int id = packet.ReadInt();
        Vector3 position = packet.ReadVector3();
        Quaternion rotation = packet.ReadQuaternion();
        float timestamp = packet.ReadFloat();

        if (GameManager._instance.TryGetObject(list, id, out ObjectManager netManager))
        {
            DynamicObjectManager dynamic = (DynamicObjectManager)netManager;

            if (dynamic.LatestTransformTimestamp < timestamp)
            {
                dynamic.Transform.position = position;
                dynamic.Transform.rotation = rotation;

                // Update timestamp
                dynamic.LatestTransformTimestamp = timestamp;
            }
        }
    }
    #endregion

    #region Character
    public static void CharacterDied(int connection, Packet packet)
    {
        ObjectList list = (ObjectList)packet.ReadByte();
        int id = packet.ReadInt();
        bool doMessage = packet.ReadBool();

        if (GameManager._instance.TryGetObject(list, id, out ObjectManager netManager))
        {
            CharacterObjectManager characterNetManager = (CharacterObjectManager)netManager;
            characterNetManager.DeathScript.Die(doMessage);
        }
    }

    public static void CharacterRevived(int connection, Packet packet)
    {
        ObjectList list = (ObjectList)packet.ReadByte();
        int id = packet.ReadInt();
        bool doMessage = packet.ReadBool();

        if (GameManager._instance.TryGetObject(list, id, out ObjectManager netManager))
        {
            CharacterObjectManager characterNetManager = (CharacterObjectManager)netManager;
            characterNetManager.DeathScript.Revive(doMessage);
        }
    }
    #endregion

    #region Enemy
    public static void SightChanged(int connection, Packet packet)
    {
        int id = packet.ReadInt();
        bool impairedSightRange = packet.ReadBool();
        bool impairedFOV = packet.ReadBool();

        if (GameManager._instance.TryGetObject(ObjectList.enemy, id, out ObjectManager netManager))
        {
            EnemyObjectManager enemyNetManager = (EnemyObjectManager)netManager;
            enemyNetManager.Character.impairedSightRange = impairedSightRange;
            enemyNetManager.Character.impairedFOV = impairedFOV;
        }
    }

    public static void StateChanged(int connection, Packet packet)
    {
        int id = packet.ReadInt();
        StateOption stateOption = (StateOption)packet.ReadByte();

        if (GameManager._instance.TryGetObject(ObjectList.enemy, id, out ObjectManager netManager))
        {
            EnemyObjectManager enemyNetManager = (EnemyObjectManager)netManager;
            enemyNetManager.Character.UpdateStateIndicator(stateOption);
        }
    }

    public static void AnimationChanged(int connection, Packet packet)
    {
        int id = packet.ReadInt();
        AnimationState stateOption = (AnimationState)packet.ReadByte();

        if (GameManager._instance.TryGetObject(ObjectList.enemy, id, out ObjectManager netManager))
        {
            EnemyObjectManager enemyNetManager = (EnemyObjectManager)netManager;
            enemyNetManager.Character.SetAnimation(stateOption);
        }
    }

    public static void DetectionConeUpdated(int connection, Packet packet)
    {
        int id = packet.ReadInt();
        float percentage = packet.ReadFloat();
        LineType color = (LineType)packet.ReadByte();
        bool changeAcceptionState = packet.ReadBool();
        float timestamp = packet.ReadFloat();

        if (GameManager._instance.TryGetObject(ObjectList.enemy, id, out ObjectManager netManager))
        {
            EnemyObjectManager enemy = (EnemyObjectManager)netManager;

            if (changeAcceptionState)
            {
                enemy.AcceptingDetectionConeUpdates = !enemy.AcceptingDetectionConeUpdates;
                enemy.Character.UpdateSightVisuals(percentage, color);
                enemy.LatestDetectionConeTimestamp = 0;
            }
            else if (enemy.AcceptingDetectionConeUpdates && enemy.LatestDetectionConeTimestamp < timestamp)
            {
                enemy.Character.UpdateSightVisuals(percentage, color);
                enemy.LatestDetectionConeTimestamp = timestamp;
            }
        }
    }
    #endregion

    #region DisposableObjects
    public static void AbilityVisualEffectCreated(int connection, Packet packet)
    {
        AbilityOption ability = (AbilityOption)packet.ReadByte();
        Vector3 position = packet.ReadVector3();

        AbilitySpawner.Instance.SpawnAtPosition(position, ability);
    }
    #endregion

    #region Player

    public static void CharacterControllerUpdate(int connection, Packet packet)
    {
        ObjectType character = (ObjectType)packet.ReadShort();
        int controllerId = packet.ReadInt();

        if (GameManager._instance.TryGetObject(ObjectList.player, (int)character, out ObjectManager objectManager))
        {
            PlayerObjectManager playerObjectManager = (PlayerObjectManager)objectManager;
            if (controllerId == Constants.noId)
            {
                playerObjectManager.Controller = null;
            }
            else
            {
                playerObjectManager.Controller = controllerId;
            }
        }
    }

    public static void Crouching(int connection, Packet packet)
    {
        ObjectType character = (ObjectType)packet.ReadShort();
        bool state = packet.ReadBool();

        if (GameManager._instance.TryGetObject(ObjectList.player, (int)character, out ObjectManager netManager))
        {
            PlayerObjectManager playerNetManager = (PlayerObjectManager)netManager;
            playerNetManager.PlayerController.IsCrouching = state;
        }
    }

    public static void Running(int connection, Packet packet)
    {
        ObjectType character = (ObjectType)packet.ReadShort();
        bool state = packet.ReadBool();

        if (GameManager._instance.TryGetObject(ObjectList.player, (int)character, out ObjectManager netManager))
        {
            PlayerObjectManager playerNetManager = (PlayerObjectManager)netManager;
            playerNetManager.PlayerController.IsRunning = state;
        }
    }

    public static void InvisibilityActivated(int connection, Packet packet)
    {
        ObjectType character = (ObjectType)packet.ReadShort();

        if (GameManager._instance.TryGetObject(ObjectList.player, (int)character, out ObjectManager netManager))
        {
            PlayerObjectManager playerNetManager = (PlayerObjectManager)netManager;
            playerNetManager.PlayerController.IsInvisible = true;
        }
    }

    public static void AbilityUsed(int connection, Packet packet)
    {
        ObjectType character = (ObjectType)packet.ReadShort();
        int abilityNum = packet.ReadInt();

        if (GameManager._instance.TryGetObject(ObjectList.player, (int)character, out ObjectManager netManager))
        {
            PlayerObjectManager playerNetManager = (PlayerObjectManager)netManager;
            playerNetManager.PlayerController.abilityLimitUsed = abilityNum;
        }
    }

    public static void ChangeInvisibility(int connection, Packet packet)
    {
        ObjectType character = (ObjectType)packet.ReadShort();
        bool state = packet.ReadBool();

        if (GameManager._instance.TryGetObject(ObjectList.player, (int)character, out ObjectManager netManager))
        {
            PlayerObjectManager playerNetManager = (PlayerObjectManager)netManager;
            playerNetManager.PlayerController._isInvisible = state;
        }
    }
    #endregion

    #region Activatable

    public static void ActivationStateChanged(int connection, Packet packet)
    {
        int id = packet.ReadInt();
        bool state = packet.ReadBool();

        if (GameManager._instance.TryGetObject(ObjectList.activatable, id, out ObjectManager objectManager))
        {
            ActivatableObjectManager activatableObjectManager = (ActivatableObjectManager)objectManager;
            activatableObjectManager.ActivatorScript.activated = state;
        }
    }

    #endregion
}

