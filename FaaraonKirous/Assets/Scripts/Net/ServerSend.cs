using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerSend
{
    #region Packets
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

    #region Enemy
    public static void EnemyCreated(int connection, int enemyId, Vector3 position)
    {
        var packet = new Packet((int)ServerPackets.enemyCreated);
        packet.Write(enemyId);
        packet.Write(position);

        Server.Instance.BeginSendPacket(connection, ChannelType.Reliable, packet);
    }

    public static void EnemyTransformUpdate(int connection, int enemyId, Vector3 position, Quaternion quaternion)
    {
        var packet = new Packet((int)ServerPackets.enemyTransform);
        packet.Write(enemyId);
        packet.Write(position);
        packet.Write(quaternion);

        Server.Instance.BeginSendPacket(connection, ChannelType.Unreliable, packet);
    }

    #endregion


    
}

