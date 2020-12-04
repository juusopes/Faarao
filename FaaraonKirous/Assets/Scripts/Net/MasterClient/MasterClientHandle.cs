using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class MasterClientHandle
{
    public static void ConnectionAccepted(int connection, Packet packet)
    {
        int sendId = packet.ReadInt();

        MasterClient.Instance.Connection.SendId = sendId;
    }

    public static void Heartbeat(int connection, Packet packet)
    {
        MasterClientSend.HeartbeatResponse();
    }
}
