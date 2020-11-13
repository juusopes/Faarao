#if UNITY_EDITOR
using ParrelSync;
#endif
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

    // TODO: Add is connected bool to should send to server
    public bool ShouldSendToServer => !IsHost;

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
                HostServer();
            }
        }
#endif
    }

    public bool HostServer()
    {
        if (Client.Instance.IsOnline || Server.Instance.IsOnline)
        {
            Debug.Log("Cannot create server if server or client is online!");
            return false;
        }
        else
        {
            Server.Instance.Start(Constants.port);

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

    public bool JoinServer(IPEndPoint endPoint = null)
    {
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
            
            Client.Instance.ConnectToServer(endPoint);

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
    }
}
