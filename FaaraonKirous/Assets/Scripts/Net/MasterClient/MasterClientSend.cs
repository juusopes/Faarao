using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class MasterClientSend
{
    public static void ServerAnnouncement(string name, bool hasPassword)
    {
        Debug.Log("Sending connection request to master server...");

        var packet = new Packet((int)MasterClientPackets.serverAnnouncement);
        packet.Write(name);
        packet.Write(hasPassword);

        MasterClient.Instance.BeginSendPacket(ChannelType.Reliable, packet);
    }

    public static void Disconnecting()
    {
        var packet = new Packet((int)MasterClientPackets.disconnecting);
        MasterClient.Instance.BeginSendPacket(ChannelType.Unreliable, packet);
    }

    public static void HeartbeatResponse()
    {
        var packet = new Packet((int)MasterClientPackets.heartbeatResponse);
        MasterClient.Instance.BeginSendPacket(ChannelType.Reliable, packet);
    }



    //public static void PlayerConnected()
    //{
    //    var packet = new Packet((int)MasterClientPackets.playerConnected);
    //    Client.Instance.BeginSendPacket(ChannelType.Reliable, packet);
    //}

    //public static void PlayerDisconnected()
    //{
    //    var packet = new Packet((int)MasterClientPackets.playerDisconnected);
    //    Client.Instance.BeginSendPacket(ChannelType.Reliable, packet);
    //}
}
