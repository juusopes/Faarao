using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class MasterServerHandle
{
    public static void ServerAnnouncement(int connection, Packet packet)
    {
        Debug.Log($"Server announcement received");

        // Add to mongo
        MasterServerManager.Instance.CreateServerObject(
            connection,
            packet.ReadString(), 
            MasterServer.Instance.Connections[connection].EndPoint.ToString(), 
            packet.ReadBool()
        );

        MasterServerSend.ConnectionAccepted(connection);
    }

    public static void HandshakeRequest(int connection, Packet packet)
    {
        Debug.Log($"Handshake request received!");

        string guidString = packet.ReadString();

        MasterServerSend.ConnectionAccepted(connection);

        MasterServerManager.Instance.DoHandshake(connection, guidString);
    }

    public static void Disconnecting(int connection, Packet packet)
    {
        MasterServer.Instance.Disconnect(connection);
    }

    public static void HeartbeatResponse(int connection, Packet packet)
    {
        // do nothing
    }



    //public static void PlayerConnected(int connection, Packet packet)
    //{
    //    MasterServer.Instance.DisconnectServer(connection);
    //}

    //public static void PlayerDisconnected(int connection, Packet packet)
    //{
    //    MasterServer.Instance.DisconnectServer(connection);
    //}

}
