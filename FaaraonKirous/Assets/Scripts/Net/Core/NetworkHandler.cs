using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public abstract class NetworkHandler
{
    public bool IsOnline { get; private set; } = false;

    protected UdpClient _socket = null;
    protected Dictionary<int, PacketHandler> _packetHandlers;
    protected CancellationTokenSource _internalUpdateCts;

    public abstract void BeginHandlePacket(int connectionId, IPEndPoint endPoint, Packet packet);
    protected abstract void OnReceiveException();
    protected abstract void InternalUpdate();
    public abstract void ConnectionTimeout(int connection);

    public NetworkSimulator Simulator { get; private set; } = null;

    public Connection MasterServer { get; protected set; } = null;

    public void SetNetworkSimulator(NetworkSimulatorConfig config)
    {
        Simulator = new NetworkSimulator(config, SendRaw);
    }

    protected bool CloseSocket()
    {
        if (_socket != null)
        {
            // TODO: Cannot close socket on client. But should close on server
            _socket.Close();
            _socket = null;

            _internalUpdateCts.Cancel();

            IsOnline = false;
            return true;
        }
        return false;
    }

    protected void StartInternalUpdate()
    {
        _internalUpdateCts = new CancellationTokenSource();
        CancellationToken token = _internalUpdateCts.Token;
        Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(Constants.updateFrequency);

                if (Simulator != null)
                {
                    Simulator.InternalUpdate();
                }
                InternalUpdate();
                if (MasterServer.EndPoint != null) MasterServer.InternalUpdate();
            }
            Debug.Log("Internal update stopped");
        });
    }

    protected void BeginReceive()
    {
        _socket.BeginReceive(ReceiveCallback, null);
        IsOnline = true;
    }

    private void ReceiveCallback(IAsyncResult result)
    {
        try
        {
            IPEndPoint endPoint = null;
            byte[] data = _socket.EndReceive(result, ref endPoint);
            _socket.BeginReceive(ReceiveCallback, null);

            // TODO: Add better error handling
            if (data.Length < Constants.intLengthInBytes) return;

            var packet = new Packet(data);
            int receiveId = packet.ReadInt();

            if (receiveId == Constants.masterServerId)
            {
                BeginHandlePacketFromMasterServer(packet);
            }
            else
            {
                BeginHandlePacket(receiveId, endPoint, packet);
            }
        }
        catch (Exception ex)
        {
            Debug.Log($"Error receiving UDP data: {ex} with message {ex.Message}");
            //OnReceiveException();
        }
    }

    public void BeginSendPacketToMasterServer(ChannelType channelType, Packet packet)
    {
        MasterServer.BeginSendPacket(channelType, packet);
    }

    public void BeginHandlePacketFromMasterServer(Packet packet)
    {
        MasterServer.BeginHandlePacket(packet);
    }

    protected void MasterServerTimeout()
    {
        ThreadManager._instance.ExecuteOnMainThread(() =>
        {
            MasterServer.Disconnect();
            MasterServer.Reset();
        });
    }

    public void SendPacket(Packet packet, IPEndPoint endPoint)
    {
        if (Simulator != null)
        {
            Simulator.Add(packet, endPoint);
        }
        else
        {
            SendRaw(packet, endPoint);
        }
    }

    public void SendRaw(Packet packet, IPEndPoint endPoint)
    {
        try
        {
            // TODO: check that connection is not null
            _socket.BeginSend(packet.ToArray(), packet.Length(), endPoint, null, null);
        }
        catch (Exception ex)
        {
            Debug.Log($"Error sending data via UDP: {ex}");
        }
    }

    public void HandlePacket(Packet packet, int connectionId)
    {
        // Read the length of the rest of the packet
        int packetLength = packet.ReadInt();
        byte[] packetBytes = packet.ReadBytes(packetLength);

        // Packet can be disposed now
        packet.Dispose();

        ThreadManager._instance.ExecuteOnMainThread(() =>
        {
            using (var newPacket = new Packet(packetBytes))
            {
                int packetType = newPacket.ReadInt();
                _packetHandlers[packetType](connectionId, newPacket);
            }
        });
    }

    protected void IgnoreRemoteHostClosedConnection()
    {
        _socket.Client.IOControl(
            (IOControlCode)Constants.sioUdpConnectionReset,
            new byte[] { 0, 0, 0, 0 },
            null
        );
    }
}
