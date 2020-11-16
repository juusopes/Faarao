using UnityEngine;

public class ServerSend
{
    #region Core
    public static void ConnectionAccepted(int connection)
    {
        var packet = new Packet((int)ServerPackets.connectionAccepted);
        packet.Write(connection);
        packet.Write("Hello there");

        Server.Instance.BeginSendPacket(connection, ChannelType.Reliable, packet);
    }

    public static void Message(int connection, string message)
    {
        var packet = new Packet((int)ServerPackets.message);
        packet.Write(message);

        Server.Instance.BeginSendPacket(connection, ChannelType.Reliable, packet);
    }

    public static void Heartbeat(int connection, long timeStamp, int lastPing)
    {
        var packet = new Packet((int)ServerPackets.heartbeat);
        packet.Write(timeStamp);
        packet.Write(lastPing);

        Server.Instance.BeginSendPacket(connection, ChannelType.Unreliable, packet);
    }

    public static void SyncPlayers(int connection)
    {
        var packet = new Packet((int)ServerPackets.syncPlayers);

        // Player count
        packet.Write(GameManager._instance.Players.Count);

        // Player info
        foreach (int id in GameManager._instance.Players.Keys)
        {
            packet.Write(id);
            packet.Write(GameManager._instance.Players[id].Name);
        }

        Server.Instance.BeginSendPacket(connection, ChannelType.Reliable, packet);
    }
    #endregion

    #region LoadAndSave
    public static void StartLoading(int? connection = null)
    {
        var packet = new Packet((int)ServerPackets.startLoading);

        if (connection.HasValue)
        {
            Server.Instance.BeginSendPacket(connection.Value, ChannelType.Reliable, packet);
        }
        else
        {
            Server.Instance.BeginSendPacketAll(ChannelType.Reliable, packet,
                ConnectionState.Connected,
                ConnectionState.SceneLoaded);
        }
        
    }

    public static void LoadScene(int index, int? connection = null)
    {
        var packet = new Packet((int)ServerPackets.loadScene);
        packet.Write(index);

        if (connection.HasValue)
        {
            Server.Instance.BeginSendPacket(connection.Value, ChannelType.Reliable, packet);
        }
        else
        {
            Server.Instance.BeginSendPacketAll(ChannelType.Reliable, packet,
                ConnectionState.Connected,
                ConnectionState.SceneLoaded);
        }
    }

    public static void EndLoading()
    {
        var packet = new Packet((int)ServerPackets.endLoading);
        Server.Instance.BeginSendPacketAll(ChannelType.Reliable, packet,
            ConnectionState.SceneLoaded,
            ConnectionState.Synced);
    }
    #endregion

    #region ObjectSyncing
    public static void SyncObject(ObjectManager netManager)
    {
        var packet = new Packet((int)ServerPackets.syncObject);
        packet.Write((byte)netManager.List);
        packet.Write(netManager.Id);
        netManager.SendSync(packet);
        Server.Instance.BeginSendPacketAll(ChannelType.Reliable, packet,
            ConnectionState.SceneLoaded, ConnectionState.Synced);
    }
    #endregion

    #region ObjectCreation
    public static void ObjectCreated(ObjectType type, int id, Vector3 position, Quaternion rotation, bool isSyncing = false)
    {
        var packet = new Packet((int)ServerPackets.objectCreated);
        packet.Write((short)type);
        packet.Write(id);
        packet.Write(position);
        packet.Write(rotation);

        if (isSyncing)
        {
            Server.Instance.BeginSendPacketAll(ChannelType.Reliable, packet, ConnectionState.SceneLoaded, 
                ConnectionState.Synced);
        }
        else
        {
            Server.Instance.BeginSendPacketAll(ChannelType.Reliable, packet, ConnectionState.Synced);
        }
    }

    public static void DisposableObjectCreated(ObjectType type, Vector3 position, Quaternion rotation)
    {
        var packet = new Packet((int)ServerPackets.disposableObjectCreated);
        packet.Write((short)type);
        packet.Write(position);
        packet.Write(rotation);

        Server.Instance.BeginSendPacketAll(ChannelType.Unreliable, packet, ConnectionState.Synced);
    }
    #endregion

    #region ObjectUpdating
    public static void UpdateObjectTransform(ObjectList list, int id, Vector3 position, Quaternion rotation)
    {
        var packet = new Packet((int)ServerPackets.updateObjectTransform);
        packet.Write((byte)list);
        packet.Write(id);
        packet.Write(position);
        packet.Write(rotation);
        packet.Write(Time.fixedTime);

        Server.Instance.BeginSendPacketAll(ChannelType.Unreliable, packet, ConnectionState.Synced);
    }
    #endregion

    #region Character
    public static void CharacterDied(ObjectList list, int id)
    {
        var packet = new Packet((int)ServerPackets.characterDied);
        packet.Write((byte)list);
        packet.Write(id);

        Server.Instance.BeginSendPacketAll(ChannelType.Reliable, packet, ConnectionState.Synced);
    }
    #endregion

    #region Enemy

    public static void SightChanged(int id, bool impairedSightRange, bool impairedFOV)
    {
        var packet = new Packet((int)ServerPackets.sightChanged);
        packet.Write(id);
        packet.Write(impairedSightRange);
        packet.Write(impairedFOV);

        Server.Instance.BeginSendPacketAll(ChannelType.Reliable, packet, ConnectionState.Synced);
    }

    public static void StateChanged(int id, StateOption stateOption)
    {
        var packet = new Packet((int)ServerPackets.stateChanged);
        packet.Write(id);
        packet.Write((byte)stateOption);

        Server.Instance.BeginSendPacketAll(ChannelType.Reliable, packet, ConnectionState.Synced);
    }

    public static void DetectionConeUpdated(int id, float percentage, LineType color, bool atExtreme)
    {
        var packet = new Packet((int)ServerPackets.detectionConeUpdated);
        packet.Write(id);
        packet.Write(percentage);
        packet.Write((byte)color);
        packet.Write(atExtreme);
        packet.Write(Time.fixedTime);

        // TODO: Send in ignoreOldUnreliable channel or send timeStamp here. OR have a channel pool for unreliables
        ChannelType channelType = atExtreme ? ChannelType.Reliable : ChannelType.Unreliable;
        Server.Instance.BeginSendPacketAll(channelType, packet, ConnectionState.Synced);
    }
    #endregion

    #region DisposableObjects
    private static void AbilityVisualEffectCreated(out Packet packet, AbilityOption ability, Vector3 position)
    {
        packet = new Packet((int)ServerPackets.abilityVisualEffectCreated);
        packet.Write((byte)ability);
        packet.Write(position);
    }

    public static void AbilityVisualEffectCreated(AbilityOption ability, Vector3 position)
    {
        AbilityVisualEffectCreated(out Packet packet, ability, position);

        Server.Instance.BeginSendPacketAll(ChannelType.Reliable, packet, ConnectionState.Synced);
    }

    public static void AbilityVisualEffectCreated(int excludeId, AbilityOption ability, Vector3 position)
    {
        AbilityVisualEffectCreated(out Packet packet, ability, position);

        Server.Instance.BeginSendPacketAllExclude(excludeId, ChannelType.Reliable, packet, ConnectionState.Synced);
    }
    #endregion

    #region Player

    public static void CharacterControllerUpdate(ObjectType character, int controllerId)
    {
        var packet = new Packet((int)ServerPackets.characterControllerUpdate);
        packet.Write((short)character);
        packet.Write(controllerId);

        Server.Instance.BeginSendPacketAll(ChannelType.Reliable, packet, ConnectionState.Synced);
    }

    public static void Crouching(ObjectType character, bool state, int? excludeId = null)
    {
        var packet = new Packet((int)ServerPackets.crouching);
        packet.Write((short)character);
        packet.Write(state);

        if (excludeId.HasValue)
        {
            Server.Instance.BeginSendPacketAllExclude(excludeId.Value, ChannelType.Reliable, packet);
        }
        else
        {
            Server.Instance.BeginSendPacketAll(ChannelType.Reliable, packet);
        }
        
    }

    public static void Running(ObjectType character, bool state, int? excludeId = null)
    {
        var packet = new Packet((int)ServerPackets.running);
        packet.Write((short)character);
        packet.Write(state);

        if (excludeId.HasValue)
        {
            Server.Instance.BeginSendPacketAllExclude(excludeId.Value, ChannelType.Reliable, packet);
        }
        else
        {
            Server.Instance.BeginSendPacketAll(ChannelType.Reliable, packet);
        }
    }
    #endregion
}

