using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

/*
public class Player
{
    public int _id;
    public IPEndPoint _endPoint;

    public UnreliableChannel _unreliableChannel;
    public ReliableChannel _reliableChannel;

    private Dictionary<ChannelType, IChannel> _channels;

    public Player(int id)
    {
        _id = id;
        _endPoint = null;
    }

    public void Connect(IPEndPoint endPoint)
    {
        _endPoint = endPoint;

        _unreliableChannel = new UnreliableChannel(Server._socket, _endPoint);
        _reliableChannel = new ReliableChannel(Server._socket, endPoint);
        _reliableChannel._id = _id;
        _unreliableChannel._id = _id;

        InitializePlayerData();

        ServerSend.ConnectionAccepted(_id);
    }

    public void SendPacket(ChannelType channelType, Packet packet)
    {
        _channels[channelType].SendPacket(packet);
    }

    public void HandlePacket(ChannelType channelType, Packet packet)
    {
        Debug.Log($"Handling packet from client {_id}");
        _channels[channelType].HandlePacket(packet);
    }

    private void InitializePlayerData()
    {
        _channels = new Dictionary<ChannelType, IChannel>()
        {
            { ChannelType.Unreliable, _unreliableChannel },
            { ChannelType.Reliable, _reliableChannel }
        };
    }

    public void Disconnect()
    {
        _endPoint = null;
        _unreliableChannel = null;
        _reliableChannel = null;
    }

    public void InternalUpdate()
    {
        if (_reliableChannel != null) _reliableChannel.InternalUpdate();

    }
}
*/