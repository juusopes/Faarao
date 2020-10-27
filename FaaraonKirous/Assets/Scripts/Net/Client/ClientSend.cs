using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSend
{

    #region Core
    public static void ConnectionRequest()
    {
        var packet = new Packet((int)ClientPackets.connectionRequest);
        Client.Instance.BeginSendPacket(ChannelType.Reliable, packet);
    }

    public static void ConnectionAcceptedReceived()
    {
        var packet = new Packet((int)ClientPackets.connectionAcceptedReceived);
        packet.Write("Happy to join server");
        Client.Instance.BeginSendPacket(ChannelType.Reliable, packet);
    }

    public static void HeartbeatReceived(long timeStamp)
    {
        var packet = new Packet((int)ClientPackets.heartbeatReceived);
        packet.Write(timeStamp);
        Client.Instance.BeginSendPacket(ChannelType.Unreliable, packet);
    }
    #endregion

    #region Abilities
    public static void AbilityUsed(AbilityOption ability, Vector3 position)
    {
        var packet = new Packet((int)ClientPackets.abilityUsed);
        packet.Write((byte)ability);
        packet.Write(position);

        Client.Instance.BeginSendPacket(ChannelType.Reliable, packet);
    }

    public static void EnemyPossessed(int id, Vector3 position)
    {
        var packet = new Packet((int)ClientPackets.enemyPossessed);
        packet.Write(id);
        packet.Write(position);

        Client.Instance.BeginSendPacket(ChannelType.Reliable, packet);
    }
    #endregion
}
