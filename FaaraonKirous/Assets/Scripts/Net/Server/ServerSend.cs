using System.Collections;
using System.Collections.Generic;
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
    #endregion

    #region ObjectSyncing
    public static void StartingObjectSync()
    {
        var packet = new Packet((int)ServerPackets.startingObjectSync);
        Server.Instance.BeginSendPacketAll(ChannelType.Reliable, packet);
    }

    public static void SyncObject(ObjectList list, int id, ObjectNetManager netManager)
    {
        var packet = new Packet((int)ServerPackets.syncObject);
        packet.Write((byte)list);
        packet.Write(id);
        netManager.SendSync(packet);
        Server.Instance.BeginSendPacketAll(ChannelType.Reliable, packet);
    }
    #endregion

    #region ObjectCreation
    public static void ObjectCreated(ObjectType type, int id, Vector3 position, Quaternion rotation)
    {
        var packet = new Packet((int)ServerPackets.objectCreated);
        packet.Write((short)type);
        packet.Write(id);
        packet.Write(position);
        packet.Write(rotation);

        Server.Instance.BeginSendPacketAll(ChannelType.Reliable, packet);
    }

    public static void DisposableObjectCreated(ObjectType type, Vector3 position, Quaternion rotation)
    {
        var packet = new Packet((int)ServerPackets.disposableObjectCreated);
        packet.Write((short)type);
        packet.Write(position);
        packet.Write(rotation);

        Server.Instance.BeginSendPacketAll(ChannelType.Unreliable, packet);
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

        Server.Instance.BeginSendPacketAll(ChannelType.Unreliable, packet);
    }
    #endregion

    #region Enemy

    public static void SightChanged(int id, bool impairedSightRange, bool impairedFOV)
    {
        var packet = new Packet((int)ServerPackets.sightChanged);
        packet.Write(id);
        packet.Write(impairedSightRange);
        packet.Write(impairedFOV);

        Server.Instance.BeginSendPacketAll(ChannelType.Reliable, packet);
    }

    public static void StateChanged(int id, StateOption stateOption)
    {
        var packet = new Packet((int)ServerPackets.stateChanged);
        packet.Write(id);
        packet.Write((byte)stateOption);

        Server.Instance.BeginSendPacketAll(ChannelType.Reliable, packet);
    }
    public static void EnemyDied(int id)
    {
        var packet = new Packet((int)ServerPackets.enemyDied);
        packet.Write(id);

        Server.Instance.BeginSendPacketAll(ChannelType.Reliable, packet);
    }

    public static void DetectionConeUpdated(int id, float percentage, LineType color, bool changeState)
    {
        var packet = new Packet((int)ServerPackets.detectionConeUpdated);
        packet.Write(id);
        packet.Write(percentage);
        packet.Write((byte)color);
        packet.Write(changeState);

        // TODO: Send in ignoreOldUnreliable channel or send timeStamp here. OR have a channel pool for unreliables
        ChannelType channelType = changeState ? ChannelType.Reliable : ChannelType.Unreliable;
        Server.Instance.BeginSendPacketAll(channelType, packet);
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

        Server.Instance.BeginSendPacketAll(ChannelType.Reliable, packet);
    }

    public static void AbilityVisualEffectCreated(int excludeId, AbilityOption ability, Vector3 position)
    {
        AbilityVisualEffectCreated(out Packet packet, ability, position);

        Server.Instance.BeginSendPacketAllExclude(excludeId, ChannelType.Reliable, packet);
    }
    #endregion


}

