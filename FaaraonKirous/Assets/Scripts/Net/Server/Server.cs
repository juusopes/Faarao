using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEditor;
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
    public Dictionary<int, ConnectionState> ConnectionStates { get; private set; }

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

    public void BeginSendPacket(int connectionId, ChannelType channelType, Packet packet, 
        ConnectionState hasFlags = ConnectionState.None, 
        ConnectionState doesNotHaveFlags = ConnectionState.None)
    {
        if (Connections[connectionId].EndPoint != null 
            && HasConnectionFlags(connectionId, hasFlags)
            && DoesNotHaveConnectionFlags(connectionId, doesNotHaveFlags)) 
        { 
            Connections[connectionId].BeginSendPacket(channelType, packet); 
        }
    }

    public void BeginSendPacketAll(ChannelType channelType, Packet packet, 
        ConnectionState hasFlags = ConnectionState.None,
        ConnectionState doesNotHaveFlags = ConnectionState.None)
    {
        foreach (Connection connection in Connections.Values)
        {
            BeginSendPacket(connection.ConnectionId, channelType, packet, hasFlags, doesNotHaveFlags);
        }
    }

    public void BeginSendPacketAllExclude(int excludeReceiveId, ChannelType channelType, Packet packet, 
        ConnectionState hasFlags = ConnectionState.None,
        ConnectionState doesNotHaveFlags = ConnectionState.None)
    {
        foreach (Connection connection in Connections.Values)
        {
            if (connection.ConnectionId != excludeReceiveId)
            {
                BeginSendPacket(connection.ConnectionId, channelType, packet, hasFlags, doesNotHaveFlags);
            }
        }
    }

    public override void BeginHandlePacket(int connectionId, IPEndPoint endPoint, Packet packet)
    {
        // Handle new connections
        if (connectionId == Constants.defaultConnectionId)  // if default ID is used, connection has not been established
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
                Connections[i].Connect(endPoint, Constants.defaultConnectionId);
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
            { (int)ClientPackets.connectionRequest, ServerHandle.ConnectionRequest },
            { (int)ClientPackets.heartbeatReceived, ServerHandle.HeartbeatReceived },
            { (int)ClientPackets.abilityUsed, ServerHandle.AbilityUsed },
            { (int)ClientPackets.enemyPossessed, ServerHandle.EnemyPossessed },
            { (int)ClientPackets.syncRequest, ServerHandle.SyncRequest },
            { (int)ClientPackets.selectCharacterRequest, ServerHandle.SelectCharacterRequest },
            { (int)ClientPackets.unselectCharacterRequest, ServerHandle.UnselectCharacterRequest },
            { (int)ClientPackets.setDestinationRequest, ServerHandle.SetDestinationRequest },
            { (int)ClientPackets.killEnemy, ServerHandle.KillEnemy },
            { (int)ClientPackets.crouching, ServerHandle.Crouching },
            { (int)ClientPackets.running, ServerHandle.Running }
        };

        // Initialize connections
        Connections = new Dictionary<int, Connection>();
        ConnectionStates = new Dictionary<int, ConnectionState>();
        for (int i = 1; i <= MaxPlayers; ++i)
        {
            Connections.Add(i, new Connection(i, Instance));
            ConnectionStates.Add(i, ConnectionState.None);
        }
    }

    #region ConnectionStates
    public bool IsSynced(int id)
    {
        return HasConnectionFlags(id, ConnectionState.Synced);
    }

    public bool HasConnectionFlags(int id, ConnectionState flags)
    {
        return (ConnectionStates[id] & flags) == flags;
    }

    public bool DoesNotHaveConnectionFlags(int id, ConnectionState flags)
    {
        return (ConnectionStates[id] & flags) == 0;
    }

    public void SetConnectionFlags(int id, ConnectionState flags)
    {
        ConnectionStates[id] = ConnectionStates[id].SetFlags(flags);
    }

    public void ResetConnectionFlags(int id, ConnectionState flags)
    {
        ConnectionStates[id] = ConnectionStates[id].ResetFlags(flags);
    }

    public void ResetConnectionFlags(ConnectionState flags,
        ConnectionState hasFlags = ConnectionState.None,
        ConnectionState doesNotHaveFlags = ConnectionState.None)
    {
        foreach (int id in ConnectionStates.Keys.ToList())
        {
            if (HasConnectionFlags(id, hasFlags)
            && DoesNotHaveConnectionFlags(id, doesNotHaveFlags))
            {
                ResetConnectionFlags(id, flags);
            }
        }
    }

    public void SetConnectionFlags(ConnectionState flags,
        ConnectionState hasFlags = ConnectionState.None,
        ConnectionState doesNotHaveFlags = ConnectionState.None)
    {
        foreach (int id in ConnectionStates.Keys.ToList())
        {
            if (HasConnectionFlags(id, hasFlags)
            && DoesNotHaveConnectionFlags(id, doesNotHaveFlags))
            {
                SetConnectionFlags(id, flags);
            }
        }
    }

    #endregion
}
