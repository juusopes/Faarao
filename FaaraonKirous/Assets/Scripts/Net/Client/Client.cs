using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public sealed class Client : NetworkHandler
{
    private static Client _instance = null;

    public static Client Instance
    {
        get
        {
            if (_instance == null) _instance = new Client();
            return _instance;
        }
    }
    public Connection Connection { get; private set; }

    public void ConnectToServer(IPEndPoint endPoint)
    {
        InitializeClientData();

        Connection.Connect(endPoint, Constants.DefaultConnectionId);

        StartInternalUpdate();

        BeginReceive();

        ClientSend.ConnectionRequest();
    }

    public void Disconnect()
    {
        if (CloseSocket()) Debug.Log("Disconnected from server.");
        NetworkManager._instance.IsConnectedToServer = false;
    }

    protected override void OnReceiveException()
    {
        Disconnect();
    }

    protected override void InternalUpdate()
    {
        if (Connection.EndPoint != null) Connection.InternalUpdate();
    }

    public void BeginSendPacket(ChannelType channelType, Packet packet)
    {
        Connection.BeginSendPacket(channelType, packet);
    }

    public override void BeginHandlePacket(int connectionId, IPEndPoint endPoint, Packet packet)
    {
        Connection.BeginHandlePacket(packet);
    }

    private void InitializeClientData()
    {
        // Initialize UDP client
        _socket = new UdpClient(0);

        // Initialize packet handlers
        _packetHandlers = new Dictionary<int, PacketHandler>()
        {
            { (int)ServerPackets.connectionAccepted, ClientHandle.ConnectionAccepted },
            { (int)ServerPackets.message, ClientHandle.Message },
            { (int)ServerPackets.heartbeat, ClientHandle.Heartbeat },
            { (int)ServerPackets.objectCreated, ClientHandle.ObjectCreated },
            { (int)ServerPackets.updateObjectTransform, ClientHandle.UpdateObjectTransform },
            { (int)ServerPackets.disposableObjectCreated, ClientHandle.DisposableObjectCreated },
            { (int)ServerPackets.startingObjectSync, ClientHandle.StartingObjectSync },
            { (int)ServerPackets.syncObject, ClientHandle.SyncObject },
            { (int)ServerPackets.sightChanged, ClientHandle.SightChanged },
            { (int)ServerPackets.abilityVisualEffectCreated, ClientHandle.AbilityVisualEffectCreated },
            { (int)ServerPackets.stateChanged, ClientHandle.StateChanged },
            { (int)ServerPackets.enemyDied, ClientHandle.EnemyDied },
            { (int)ServerPackets.detectionConeUpdated, ClientHandle.DetectionConeUpdated },
            { (int)ServerPackets.loadScene, ClientHandle.LoadScene },
            { (int)ServerPackets.endLoading, ClientHandle.EndLoading },
            { (int)ServerPackets.changeToCharacter, ClientHandle.ChangeToCharacter }
        };

        // Initialize connection
        Connection = new Connection(Constants.DefaultConnectionId, Instance);
    }

    
}