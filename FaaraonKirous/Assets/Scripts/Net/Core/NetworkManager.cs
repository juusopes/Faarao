﻿using ParrelSync;
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

    // For testing
    [SerializeField]
    private bool _willHostServer;

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
        }
    }

    private void Start()
    {
        if (ClonesManager.IsClone())
        {
            // Automatically connect to local host if this is the clone editor
            JoinServer();
        }
        else
        {
            if (_willHostServer)
            {
                // Automatically start server if this is the original editor
                HostServer();
            }
        }
    }

    public void HostServer()
    {
        if (Client.Instance.IsOnline || Server.Instance.IsOnline)
        {
            Debug.Log("Cannot create server if server or client is online!");
        }
        else
        {
            Server.Instance.Start(26950);
        }
    }

    public void JoinServer()
    {
        if (Client.Instance.IsOnline || Server.Instance.IsOnline)
        {
            Debug.Log("Cannot join server if server or client is online!");
        }
        else
        {
            IsHost = false;
            // TODO: Server IP and port should be given through text fields
            string serverIp = "127.0.0.1";
            int serverPort = 26950;
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(serverIp), serverPort);
            Client.Instance.ConnectToServer(ipEndPoint);
        }
    }

    private void OnApplicationQuit()
    {
        Server.Instance.Stop();
        Client.Instance.Disconnect();
    }
}
