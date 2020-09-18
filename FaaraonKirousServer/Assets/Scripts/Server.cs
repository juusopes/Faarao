﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Server
{
    public static int MaxPlayers { get; private set; }
    public static int Port { get; private set; }

    public static Dictionary<int, Client> _clients = new Dictionary<int, Client>();
    public delegate void PacketHandler(int id, Packet packet);
    public static Dictionary<int, PacketHandler> _packetHandlers;

    private static TcpListener _tcpListener;
    private static UdpClient _udpListener;

    public static void Start(int maxPlayers, int port)
    {
        MaxPlayers = maxPlayers;
        Port = port;

        Debug.Log("Starting server...");
        InitializeServerData();

        _tcpListener = new TcpListener(IPAddress.Any, Port);
        _tcpListener.Start();
        _tcpListener.BeginAcceptTcpClient(new AsyncCallback(TcpConnectCallback), null);

        _udpListener = new UdpClient(Port);
        _udpListener.BeginReceive(UdpReceiveCallback, null);

        Debug.Log($"Server started on {Port}.");
    }

    public static void Stop()
    {
        _tcpListener.Stop();
        _udpListener.Close();
    }

    private static void TcpConnectCallback(IAsyncResult result)
    {
        TcpClient client = _tcpListener.EndAcceptTcpClient(result);
        _tcpListener.BeginAcceptTcpClient(new AsyncCallback(TcpConnectCallback), null);
        Debug.Log($"Incoming connection from {((IPEndPoint)client.Client.RemoteEndPoint).Address} " +
            $"with port {((IPEndPoint)client.Client.RemoteEndPoint).Port} and endpoint {client.Client.RemoteEndPoint}...");

        for (int i = 1; i <= MaxPlayers; ++i)
        {
            if (_clients[i]._tcp._socket == null)
            {
                _clients[i]._tcp.Connect(client);
                return;
            }
        }
        Debug.Log($"{client.Client.RemoteEndPoint} failed to connect: Server full!");
    }


    private static void UdpReceiveCallback(IAsyncResult result)
    {
        try
        {
            IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = _udpListener.EndReceive(result, ref clientEndPoint);
            _udpListener.BeginReceive(UdpReceiveCallback, null);

            if (data.Length < Constants.intLengthInBytes)
            {
                return;
            }

            using (var packet = new Packet(data))
            {
                int clientId = packet.ReadInt();

                if (clientId == 0) return;

                if (_clients[clientId]._udp._endPoint == null)
                {
                    _clients[clientId]._udp.Connect(clientEndPoint);
                    return;
                }

                if (_clients[clientId]._udp._endPoint.ToString() == clientEndPoint.ToString())
                {
                    _clients[clientId]._udp.HandleData(packet);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log($"Error receiving UDP data: {ex}");
        }
    }

    public static void SendUdpData(IPEndPoint clientEndPoint, Packet packet)
    {
        try
        {
            if (clientEndPoint == null) return;
            _udpListener.BeginSend(packet.ToArray(), packet.Length(), clientEndPoint, null, null);
        }
        catch (Exception ex)
        {
            Debug.Log($"Error sending data to {clientEndPoint} via UDP: {ex}");
        }
    }

    private static void InitializeServerData()
    {
        for (int i = 1; i <= MaxPlayers; ++i)
        {
            _clients.Add(i, new Client(i));
        }

        _packetHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived },
                { (int)ClientPackets.udpTestReceived, ServerHandle.UdpTestReceived },
                { (int)ClientPackets.playerMovement, ServerHandle.PlayerMovement },
                { (int)ClientPackets.playerShoot, ServerHandle.PlayerShoot },
                { (int)ClientPackets.playerThrowItem, ServerHandle.PlayerThrowItem }
            };

        Debug.Log("Initialized packets.");
    }
}
