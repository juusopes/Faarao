using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerUIManager : MonoBehaviour
{
    public static MultiplayerUIManager _instance;

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

    [SerializeField]
    private GameObject _multiplayerMenu = null;
    [SerializeField]
    private InputField _ipAddress = null;
    [SerializeField]
    private InputField _port = null;

    private void Start()
    {
        InitMenu();
    }

    private void InitMenu()
    {
        OpenMultiplayerMenu();
    }

    private void OpenMultiplayerMenu()
    {
        _multiplayerMenu.SetActive(true);
    }

    public void Connect()
    {
        if (!IPAddress.TryParse(_ipAddress.text, out IPAddress address))
        {
            // TODO: Add popup window
            Debug.Log("Invalid ip address");
            return;
        }
        if (!int.TryParse(_port.text, out int port))
        {
            // TODO: Add popup window
            Debug.Log("Invalid port");
            return;
        }

        IPEndPoint endPoint = new IPEndPoint(address, port);

        if (NetworkManager._instance.JoinServer(endPoint))  // Should work always
        {
            _multiplayerMenu.SetActive(false);
            
        }
    }

    public void Host()
    {
        if (NetworkManager._instance.HostServer())
        {
            _multiplayerMenu.SetActive(false);
        }

        GameManager._instance.LoadLevel(1);
    }
}
