using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MasterServerSend
{
    public static void Heartbeat()
    {
        var packet = new Packet((int)MasterServerPackets.heartbeat);

        MasterServer.Instance.BeginSendPacketAll(ChannelType.Reliable, packet);
    }

    public static void ConnectionAccepted(int connection)
    {
        var packet = new Packet((int)MasterServerPackets.connectionAccepted);
        packet.Write(connection);

        MasterServer.Instance.BeginSendPacket(connection, ChannelType.Reliable, packet);
    }

    public static void Handshake(int connection, string endpoint)
    {
        var packet = new Packet((int)MasterServerPackets.handshake);
        packet.Write(endpoint);

        MasterServer.Instance.BeginSendPacket(connection, ChannelType.Reliable, packet);
    }
}

