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
    // TESTING
    private static System.Random rand = new System.Random();
    // END TESTING

    public bool IsOnline { get; private set; } = false;

    protected UdpClient _socket = null;
    protected Dictionary<int, PacketHandler> _packetHandlers;
    protected CancellationTokenSource _internalUpdateCts;

    public abstract void BeginHandlePacket(int connectionId, IPEndPoint endPoint, Packet packet);
    protected abstract void OnReceiveException();
    protected abstract void InternalUpdate();

    protected bool CloseSocket()
    {
        if (_socket != null)
        {
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
                InternalUpdate();
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

            BeginHandlePacket(receiveId, endPoint, packet);
        }
        catch (Exception ex)
        {
            Debug.Log($"Error receiving UDP data: {ex}");
            Debug.Log($"Error receiving UDP data: {ex.Message}");
            // Cannot close...
            //OnReceiveException();
        }
    }
    

    public void SendPacket(Packet packet, IPEndPoint endPoint)
    {
        try
        {
            // TESTING
            //int n = rand.Next(0, 100);
            //if (n < 20)
            //{
            //    Debug.Log($"Simulated packet loss");
            //    return;
            //}
            // END TESTING

            _socket.BeginSend(packet.ToArray(), packet.Length(), endPoint, null, null);
            //Debug.Log("Packet sent succesfully");
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

        ThreadManager.ExecuteOnMainThread(() =>
        {
            using (var newPacket = new Packet(packetBytes))
            {
                int packetType = newPacket.ReadInt();
                _packetHandlers[packetType](connectionId, newPacket);
            }
        });
    }
}
