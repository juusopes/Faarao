using System.Collections;
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

        NetworkManager._instance.IsConnectedToServer = true;
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
        long timestamp = packet.ReadLong();

        if (GameManager._instance.TryGetObject(list, id, out ObjectNetManager netManager))
        {
            if (netManager.LatestTransformTimestamp < timestamp)
            {
                netManager.Transform.position = position;
                netManager.Transform.rotation = rotation;

                // Update timestamp
                netManager.LatestTransformTimestamp = timestamp;
            }
        }
    }
    #endregion

    #region Enemy
    public static void SightChanged(int connection, Packet packet)
    {
        int id = packet.ReadInt();
        bool impairedSightRange = packet.ReadBool();
        bool impairedFOV = packet.ReadBool();

        if (GameManager._instance.TryGetObject(ObjectList.enemy, id, out ObjectNetManager netManager))
        {
            EnemyNetManager enemyNetManager = (EnemyNetManager)netManager;
            enemyNetManager.Character.impairedSightRange = impairedSightRange;
            enemyNetManager.Character.impairedFOV = impairedFOV;
        }
    }

    public static void StateChanged(int connection, Packet packet)
    {
        int id = packet.ReadInt();
        StateOption stateOption = (StateOption)packet.ReadByte();

        if (GameManager._instance.TryGetObject(ObjectList.enemy, id, out ObjectNetManager netManager))
        {
            EnemyNetManager enemyNetManager = (EnemyNetManager)netManager;
            enemyNetManager.Character.UpdateStateIndicator(stateOption);
        }
    }

    public static void EnemyDied(int connection, Packet packet)
    {
        int id = packet.ReadInt();

        if (GameManager._instance.TryGetObject(ObjectList.enemy, id, out ObjectNetManager netManager))
        {
            EnemyNetManager enemyNetManager = (EnemyNetManager)netManager;
            enemyNetManager.Character.Die();
        }
    }

    public static void DetectionConeUpdated(int connection, Packet packet)
    {
        int id = packet.ReadInt();
        float percentage = packet.ReadFloat();
        LineType color = (LineType)packet.ReadByte();
        bool changeAcceptionState = packet.ReadBool();
        long timestamp = packet.ReadLong();

        if (GameManager._instance.TryGetObject(ObjectList.enemy, id, out ObjectNetManager netManager))
        {
            EnemyNetManager enemy = (EnemyNetManager)netManager;

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

}

