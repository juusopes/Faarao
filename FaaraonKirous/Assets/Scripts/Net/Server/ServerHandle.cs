using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerHandle
{
    public static void ConnectionRequest(int connection, Packet packet)
    {
        ServerSend.ConnectionAccepted(connection);
    }

    public static void ConnectionAcceptedReceived(int connection, Packet packet)
    {
        string msg = packet.ReadString();

        Debug.Log($"Connection acceptance received. Contains message: {msg}");

        // For testing
        //for (int i = 1; i <= 100; ++i)
        //{
        //    ServerSend.Message(connection, i.ToString());
        //}

        // TODO: Pause game here. And wait that all clients are paused too

        // TODO: This should propably be done by the network manager
        GameManager._instance.SyncAllObjects();
    }

    public static void HeartbeatReceived(int connection, Packet packet)
    {
        long timeStamp = packet.ReadLong();
        // TODO: The ping is fundamentally dependant on the fixedTime frequency, and the speed of handling other requests
        // TODO: int overflow might occur here
        int ping = (int)((DateTime.Now.Ticks - timeStamp) / TimeSpan.TicksPerMillisecond);
        // TODO: Check that connection is not null
        Server.Instance.Connections[connection].Ping = ping;
    }
}
