using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSend
{

    #region Packets
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
}
