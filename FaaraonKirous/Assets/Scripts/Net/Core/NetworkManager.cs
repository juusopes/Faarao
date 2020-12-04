#if UNITY_EDITOR
using ParrelSync;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager _instance;

    public bool IsHost { get; private set; } = true;

    public bool IsConnectedToServer { get; set; } = false;

    public bool ShouldSendToClient => Server.Instance.IsOnline;

    public bool ShouldSendToServer => !IsHost && Client.Instance.IsOnline;

    public bool IsConnectedToMasterServer => MasterClient.Instance.IsOnline;


    // For testing
    public bool Testing => _testing;

    [SerializeField]
    private bool _testing = false;
    [SerializeField]
    private bool _simulateNetwork = false;
    [SerializeField]
    private float _simulationDropPercentage = 0;
    [SerializeField]
    private int _simulationMinLatency = 0;
    [SerializeField]
    private int _simulationMaxLatency = 0;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
            return;
        }
    }

    private void Start()
    {
#if UNITY_EDITOR
        if (Testing)
        {

            if (ClonesManager.IsClone())
            {
                // Automatically connect to local host if this is the clone editor
                JoinServer();
            }
            else
            {
                // Automatically start server if this is the original editor
                HostServer("placeholder");
            }
        }
#endif
    }

    public bool HostServer(string name, string password = null)
    {
        if (Client.Instance.IsOnline || Server.Instance.IsOnline)
        {
            MessageLog.Instance.AddMessage("Already hosting or connected", Color.red);
            return false;
        }
        else
        {
            Server.Instance.Start(Constants.port, name, password);

            // For testing
            if (_simulateNetwork)
            {
                Server.Instance.SetNetworkSimulator(new NetworkSimulatorConfig
                {
                    DropPercentage = _simulationDropPercentage,
                    MinLatency = _simulationMinLatency,
                    MaxLatency = _simulationMaxLatency
                });
            }

            return true;
        }
    }

    public void AttemptHandshake(Guid guid, IPEndPoint endPoint, string password = null)
    {
        Debug.Log("Attempting handshake");

        JoinServer(endPoint, password);
        ClientSend.HandshakeRequest(guid);
    }

    public bool JoinServer(IPEndPoint endPoint = null, string password = null)
    {
        Debug.Log("Trying to join server");

        if (Client.Instance.IsOnline || Server.Instance.IsOnline)
        {
            Debug.Log("Cannot join server if server or client is online!");
            return false;
        }
        else
        {
            IsHost = false;

            if (endPoint == null)
            {
                string serverIp = Constants.ip;
                int serverPort = Constants.port;
                endPoint = new IPEndPoint(IPAddress.Parse(serverIp), serverPort);
            }
            
            Client.Instance.ConnectToServer(endPoint, GameManager._instance.GetName(), password);

            // For testing
            if (_simulateNetwork)
            {
                Client.Instance.SetNetworkSimulator(new NetworkSimulatorConfig
                {
                    DropPercentage = _simulationDropPercentage,
                    MinLatency = _simulationMinLatency,
                    MaxLatency = _simulationMaxLatency
                });
            }

            return true;
        }
    }

    private void OnApplicationQuit()
    {
        Server.Instance.Stop();
        Client.Instance.Disconnect();
        MasterClient.Instance.Disconnect();
    }

    public void ResetNetworking()
    {
        Server.Instance.Stop();
        Client.Instance.Disconnect();
        MasterClient.Instance.Disconnect();

        IsHost = true;
        IsConnectedToServer = false;
        
    }
}
