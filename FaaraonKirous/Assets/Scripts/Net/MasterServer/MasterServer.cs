using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public sealed class MasterServer : NetworkHandler
{
    private static MasterServer _instance = null;

    public static MasterServer Instance
    {
        get
        {
            if (_instance == null) _instance = new MasterServer();
            return _instance;
        }
    }

    public int MaxServers { get; private set; } = Constants.maxServers;
    public int Port { get; private set; } = Constants.masterServerPort;

    public Dictionary<int, Connection> Servers { get; private set; }

    public void Start()
    {
        Debug.Log("Starting server...");

        InitializeServerData();

        StartInternalUpdate();

        BeginReceive();

        MasterServerManager.Instance.StartHeartbeats();

        Debug.Log($"Master server started on port {Port}");
    }

    protected override void InternalUpdate()
    {
        foreach (Connection connection in Servers.Values)
        {
            if (connection.EndPoint != null) connection.InternalUpdate();
        }
    }
    public override void ConnectionTimeout(int id)
    {
        DisconnectServer(id);
    }

    public void DisconnectServer(int id)
    {
        // Check that server is not already disconnected
        if (Servers[id].EndPoint == null) return;

        // Handle disconnect in main thread
        ThreadManager._instance.ExecuteOnMainThread(() =>
        {
            // Disconnect
            Servers[id].Disconnect();

            // Reset connection internals
            Servers[id].Reset();

            MasterServerManager.Instance.RemoveServerObject(id);
        });
    }

    public void Stop()
    {
        if (_socket == null) return;

        // TODO: Send master server shut down message

        if (CloseSocket()) Debug.Log("Server stopped.");
    }

    protected override void OnReceiveException()
    {
        Stop();
    }

    public void BeginSendPacket(int id, ChannelType channelType, Packet packet)
    {
        if (Servers[id].EndPoint != null)
        {
            Servers[id].BeginSendPacket(channelType, packet);
        }
    }

    public void BeginSendPacketAll(ChannelType channelType, Packet packet)
    {
        foreach (Connection connection in Servers.Values)
        {
            BeginSendPacket(connection.ConnectionId, channelType, packet);
        }
    }

    public void BeginSendPacketAllExclude(int excludeReceiveId, ChannelType channelType, Packet packet)
    {
        foreach (Connection connection in Servers.Values)
        {
            if (connection.ConnectionId != excludeReceiveId)
            {
                BeginSendPacket(connection.ConnectionId, channelType, packet);
            }
        }
    }

    public override void BeginHandlePacket(int id, IPEndPoint endPoint, Packet packet)
    {
        // Handle new connections
        if (id == Constants.defaultConnectionId)  // if default ID is used, connection has not been established
        {
            AddConnection(endPoint, packet);
            return;
        }

        // Validate endpoint
        if (Servers[id].EndPoint != null && endPoint.ToString() != Servers[id].EndPoint.ToString())
        {
            Debug.Log("Receive ID doesn't match endpoint! Packet discarded.");
            return;
        }
        Servers[id].BeginHandlePacket(packet);
    }

    private void AddConnection(IPEndPoint endPoint, Packet packet)
    {
        // Handle packets from server if it has not updated its sendId
        for (int i = 1; i <= MaxServers; ++i)
        {
            if (Servers[i].EndPoint != null && Servers[i].EndPoint.ToString() == endPoint.ToString())
            {
                Servers[i].BeginHandlePacket(packet);
                return;
            }
        }

        Debug.Log($"Incoming connection form {endPoint}...");

        // Allocate ID and connect
        for (int i = 1; i <= MaxServers; ++i)
        {
            if (Servers[i].EndPoint == null)
            {
                Debug.Log($"Connection receives id {i}");
                Servers[i].Connect(endPoint, Constants.defaultConnectionId);
                Servers[i].BeginHandlePacket(packet);
                return;
            }
        }
    }

    private void InitializeServerData()
    {
        // Initialize UDP client
        _socket = new UdpClient(Port);
        IgnoreRemoteHostClosedConnection();

        // Initialize packet handler
        _packetHandlers = new Dictionary<int, PacketHandler>()
        {
            { (int)MasterClientPackets.connectionRequest, MasterServerHandle.ConnectionRequest },
            { (int)MasterClientPackets.disconnecting, MasterServerHandle.Disconnecting },
            { (int)MasterClientPackets.heartbeatResponse, MasterServerHandle.HeartbeatResponse }
        };

        // Initialize connections
        Servers = new Dictionary<int, Connection>();
        for (int i = 1; i <= MaxServers; ++i)
        {
            Servers.Add(i, new Connection(i, Instance));
        }
    }
}
