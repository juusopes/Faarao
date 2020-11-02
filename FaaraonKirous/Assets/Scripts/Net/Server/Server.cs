using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public sealed class Server : NetworkHandler
{
    private static Server _instance = null;

    public static Server Instance
    {
        get
        {
            if (_instance == null) _instance = new Server();
            return _instance;
        }
    }

    public int MaxPlayers { get; private set; } = Constants.maxPlayers;
    public int Port { get; private set; }

    public Dictionary<int, Connection> Connections { get; private set; }

    public void Start(int port)
    {
        Debug.Log("Starting server...");

        Port = port;

        InitializeServerData();

        StartInternalUpdate();

        BeginReceive();

        StartHeartbeats();

        Debug.Log($"Server started on {Port}.");
    }

    public void StartHeartbeats()
    {
        CancellationToken token = _internalUpdateCts.Token;
        Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(Constants.heartbeatFrequency);

                long timeStamp = DateTime.Now.Ticks;
                foreach (Connection connection in Connections.Values)
                {
                    if (connection.EndPoint != null)
                    {
                        ServerSend.Heartbeat(connection.ConnectionId, timeStamp, connection.Ping);
                    }
                }
                
            }
            Debug.Log("Heartbeats stopped");
        });
    }

    public void Stop()
    {
        if (CloseSocket()) Debug.Log("Server stopped.");
    }

    protected override void OnReceiveException()
    {
        Stop();
    }

    protected override void InternalUpdate()
    {
        foreach (Connection connection in Connections.Values)
        {
            if (connection.EndPoint != null) connection.InternalUpdate();
        }
    }

    public void BeginSendPacket(int connectionId, ChannelType channelType, Packet packet)
    {
        if (Connections[connectionId].EndPoint != null) Connections[connectionId].BeginSendPacket(channelType, packet);
    }

    public void BeginSendPacketAll(ChannelType channelType, Packet packet)
    {
        // TODO: Check that connection has connected completely!!
        foreach (Connection connection in Connections.Values)
        {
            if (connection.EndPoint != null) connection.BeginSendPacket(channelType, packet);
        }
    }

    public void BeginSendPacketAllExclude(int excludeReceiveId, ChannelType channelType, Packet packet)
    {
        foreach (Connection connection in Connections.Values)
        {
            if (connection.EndPoint != null && connection.ConnectionId != excludeReceiveId)
            {
                connection.BeginSendPacket(channelType, packet);
            }
        }
    }

    public override void BeginHandlePacket(int connectionId, IPEndPoint endPoint, Packet packet)
    {
        // Handle new connections
        if (connectionId == Constants.DefaultConnectionId)  // if default ID is used, connection has not been established
        {
            ConnectClient(endPoint, packet);
            return;
        }

        // Validate endpoint
        if (Connections[connectionId].EndPoint != null && endPoint.ToString() != Connections[connectionId].EndPoint.ToString())
        {
            Debug.Log("Receive ID doesn't match endpoint! Packet discarded.");
            return;
        }
        Connections[connectionId].BeginHandlePacket(packet);
    }

    private void ConnectClient(IPEndPoint endPoint, Packet packet)
    {
        // Handle resent connection requests and acknowledgements (sendId has not been updated)
        for (int i = 1; i <= MaxPlayers; ++i)
        {
            if (Connections[i].EndPoint != null && Connections[i].EndPoint.ToString() == endPoint.ToString())
            {
                Connections[i].BeginHandlePacket(packet);
                return;
            }
        }

        Debug.Log($"Incoming connection form {endPoint}...");

        // Allocate ID and connect
        for (int i = 1; i <= MaxPlayers; ++i)
        {
            if (Connections[i].EndPoint == null)
            {
                Connections[i].Connect(endPoint, Constants.DefaultConnectionId);
                Connections[i].BeginHandlePacket(packet);
                return;
            }
        }
    }

    private void InitializeServerData()
    {
        // Initialize UDP client
        _socket = new UdpClient(Port);

        // Initialize packet handler
        _packetHandlers = new Dictionary<int, PacketHandler>()
        {
            { (int)ClientPackets.connectionAcceptedReceived, ServerHandle.ConnectionAcceptedReceived },
            { (int)ClientPackets.connectionRequest, ServerHandle.ConnectionRequest },
            { (int)ClientPackets.heartbeatReceived, ServerHandle.HeartbeatReceived },
            { (int)ClientPackets.abilityUsed, ServerHandle.AbilityUsed },
            { (int)ClientPackets.enemyPossessed, ServerHandle.EnemyPossessed }
        };

        // Initialize connections
        Connections = new Dictionary<int, Connection>();
        for (int i = 1; i <= MaxPlayers; ++i)
        {
            Connections.Add(i, new Connection(i, Instance));
        }
    }

    
}
