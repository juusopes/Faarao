using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class UnreliableChannel : IChannel
{
    private readonly Connection _connection;

    public UnreliableChannel(Connection connection)
    {
        _connection = connection;
    }

    public void BeginSendPacket(Packet packet)
    {
        _connection.SendPacket(packet, ChannelType.Unreliable);
    }

    public void BeginHandlePacket(Packet packet)
    {
        _connection.HandlePacket(packet);
    }

    public void InternalUpdate(out bool timeout)
    {
        // Do nothing
        timeout = false;
    }
}
