using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class MasterClient : NetworkHandler
{
    private static MasterClient _instance = null;

    public static MasterClient Instance
    {
        get
        {
            if (_instance == null) _instance = new MasterClient();
            return _instance;
        }
    }

    public Connection Connection { get; private set; }

    public bool ConnectToServer(string name, bool hasPassword)
    {
        Debug.Log("Connecting to master server...");

        // Try get master server IP
        string masterServerIP = NetTools.GetMasterServerIP();
        if (masterServerIP == null) return false;
                
        // Create endpoint
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(masterServerIP), Constants.masterServerPort);

        InitializeClientData();

        Connection.Connect(endPoint, Constants.defaultConnectionId);

        StartInternalUpdate();

        BeginReceive();

        MasterClientSend.ServerAnnouncement(name, hasPassword);

        return true;
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

    public override void ConnectionTimeout(int connection)
    {
        Disconnect();
    }

    public void Disconnect()
    {
        if (Connection == null || Connection.EndPoint == null) return;

        MasterClientSend.Disconnecting();

        if (!CloseSocket()) return;

        ThreadManager._instance.ExecuteOnMainThread(() =>
        {
            MessageLog.Instance.AddMessage($"Disconnected from master server", Constants.messageColorNetworking);
        });
    }

    protected override void OnReceiveException()
    {
        Disconnect();
    }

    private void InitializeClientData()
    {
        // Initialize UDP client
        _socket = new UdpClient(0);
        IgnoreRemoteHostClosedConnection();

        // Initialize packet handlers
        _packetHandlers = new Dictionary<int, PacketHandler>()
        {
            { (int)MasterServerPackets.connectionAccepted, MasterClientHandle.ConnectionAccepted },
            { (int)MasterServerPackets.heartbeat, MasterClientHandle.Heartbeat }
        };

        // Initialize connection
        Connection = new Connection(Constants.defaultConnectionId, Instance);
    }
}
