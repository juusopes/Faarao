using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class Connection
{
    /// <summary>The connection's ID on this end.</summary>
    public int ConnectionId { get; private set; }
    /// <summary>The connection's ID on the other end.</summary>
    public int SendId { get { return _sendId; } set { _sendId = value; } }
    /// <summary>The endpoint of the connection. Null endpoint means that there is no connection.</summary>
    public IPEndPoint EndPoint { get; private set; } = null;
    public int Ping { get { return _ping; } set { _ping = value; } }

    // Volatile keyword is used here to prevent some cases of threads not retrieving the latest value.
    // Might not be necessary though.
    private volatile int _sendId;
    private volatile int _ping;

    private readonly NetworkHandler _networkHandler;
    private readonly Dictionary<ChannelType, IChannel> _channels = new Dictionary<ChannelType, IChannel>();

    public Connection(int connectionId, NetworkHandler networkHandler)
    {
        ConnectionId = connectionId;
        _networkHandler = networkHandler;
    }

    public void Connect(IPEndPoint endPoint, int sendId)
    {
        EndPoint = endPoint;
        SendId = sendId;
        Ping = Constants.defaultPing;

        // Create channels
        _channels.Add(ChannelType.Unreliable, new UnreliableChannel(this));
        _channels.Add(ChannelType.Reliable, new ReliableChannel(this));
    }

    public void Disconnect()
    {
        EndPoint = null;
        _channels.Clear();
    }

    public void InternalUpdate()
    {
        foreach (IChannel channel in _channels.Values)
        {
            channel.InternalUpdate(out bool timeout);
            if (timeout) Disconnect();
        }
    }

    public void BeginSendPacket(ChannelType channelType, Packet packet)
    {
        // TODO: Length insertion should be done elsewhere maybe
        packet.WriteLength();
        _channels[channelType].BeginSendPacket(packet);
    }

    public void SendPacket(Packet packet, ChannelType channelType, bool insertHeader = true)
    {
        if (insertHeader)
        {
            // Insert connection header
            packet.InsertByte((byte)channelType);
            packet.InsertInt(SendId);
        }

        _networkHandler.SendPacket(packet, EndPoint);
    }

    public void BeginHandlePacket(Packet packet)
    {
        Debug.Log($"Handling packet from connection {ConnectionId}");

        ChannelType channelType = (ChannelType)packet.ReadByte();
        _channels[channelType].BeginHandlePacket(packet);
    }

    public void HandlePacket(Packet packet)
    {
        _networkHandler.HandlePacket(packet, ConnectionId);
    }
}
