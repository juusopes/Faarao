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

    public void ConnectToServer(IPEndPoint endPoint, string name, string password)
    {
        Debug.Log("ConnectToServer");

        InitializeClientData();

        Connection.Connect(endPoint, Constants.defaultConnectionId);

        StartInternalUpdate();

        BeginReceive();

        ClientSend.ConnectionRequest(name, password);
    }

    public override void ConnectionTimeout(int connection)
    {
        if (connection == Constants.masterServerId)
        {
            MasterServerTimeout();
        }
        else
        {
            Disconnect();
        }
    }

    public void Disconnect()
    {
        if (Connection == null || Connection.EndPoint == null) return;

        ClientSend.Disconnecting();

        if (!CloseSocket()) return;

        ThreadManager._instance.ExecuteOnMainThread(() =>
        {
            MessageLog.Instance.AddMessage($"Disconnected from server", Constants.messageColorNetworking);
            GameManager._instance.ExitToMainMenu();
        });
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
        IgnoreRemoteHostClosedConnection();

        // Initialize packet handlers
        _packetHandlers = new Dictionary<int, PacketHandler>()
        {
            { (int)ServerPackets.connectionAccepted, ClientHandle.ConnectionAccepted },
            { (int)ServerPackets.message, ClientHandle.Message },
            { (int)ServerPackets.heartbeat, ClientHandle.Heartbeat },
            { (int)ServerPackets.objectCreated, ClientHandle.ObjectCreated },
            { (int)ServerPackets.updateObjectTransform, ClientHandle.UpdateObjectTransform },
            { (int)ServerPackets.disposableObjectCreated, ClientHandle.DisposableObjectCreated },
            { (int)ServerPackets.syncObject, ClientHandle.SyncObject },
            { (int)ServerPackets.sightChanged, ClientHandle.SightChanged },
            { (int)ServerPackets.abilityVisualEffectCreated, ClientHandle.AbilityVisualEffectCreated },
            { (int)ServerPackets.stateChanged, ClientHandle.StateChanged },
            { (int)ServerPackets.animationChanged, ClientHandle.AnimationChanged },
            { (int)ServerPackets.characterDied, ClientHandle.CharacterDied },
            { (int)ServerPackets.detectionConeUpdated, ClientHandle.DetectionConeUpdated },
            { (int)ServerPackets.loadScene, ClientHandle.LoadScene },
            { (int)ServerPackets.startLoading, ClientHandle.StartLoading },
            { (int)ServerPackets.endLoading, ClientHandle.EndLoading },
            { (int)ServerPackets.characterControllerUpdate, ClientHandle.CharacterControllerUpdate },
            { (int)ServerPackets.crouching, ClientHandle.Crouching },
            { (int)ServerPackets.running, ClientHandle.Running },
            { (int)ServerPackets.syncPlayers, ClientHandle.SyncPlayers },
            { (int)ServerPackets.playerConnected, ClientHandle.PlayerConnected },
            { (int)ServerPackets.playerDisconnected, ClientHandle.PlayerDisconnected },
            { (int)ServerPackets.serverStopped, ClientHandle.ServerStopped },
            { (int)ServerPackets.activationStateChanged, ClientHandle.ActivationStateChanged },
            { (int)ServerPackets.characterRevived, ClientHandle.CharacterRevived },
            { (int)MasterServerPackets.connectionAccepted, ClientHandle.ConnectionAcceptedMaster },
            { (int)MasterServerPackets.heartbeat, ClientHandle.HeartbeatMaster },
        };

        // Initialize connection
        Connection = new Connection(Constants.defaultConnectionId, Instance);
        MasterServer = new Connection(Constants.masterServerId, Instance);
    }

    
}