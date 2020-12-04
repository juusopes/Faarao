using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class MasterServerHandle
{
    public static void ConnectionRequest(int connection, Packet packet)
    {
        Debug.Log($"Server request received!");

        // Add to mongo
        MasterServerManager.Instance.CreateServerObject(
            connection,
            packet.ReadString(), 
            MasterServer.Instance.Servers[connection].EndPoint.ToString(), 
            packet.ReadBool()
        );

        // TODO: Create key for server

        MasterServerSend.ConnectionAccepted(connection);
    }

    public static void Disconnecting(int connection, Packet packet)
    {
        MasterServer.Instance.DisconnectServer(connection);
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
