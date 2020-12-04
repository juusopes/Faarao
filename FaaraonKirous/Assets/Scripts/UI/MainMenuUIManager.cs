using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUIManager : MonoBehaviour
{
    public static MainMenuUIManager Instance { get; private set; }

    [SerializeField]
    private GameObject _menu;

    [SerializeField]
    private InputField _ipAddress;

    [SerializeField]
    private InputField _port;

    [SerializeField]
    private InputField _password;

    [SerializeField]
    private InputField _guid;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    public void Open()
    {
        _menu.SetActive(true);
    }

    public void Close()
    {
        _menu.SetActive(false);
    }

    public void Connect()
    {
        if (!IPAddress.TryParse(_ipAddress.text, out IPAddress ip))
        {
            MessageLog.Instance.AddMessage("Not valid ip!", Color.red);
            return;
        }
        if (!int.TryParse(_port.text, out int port))
        {
            MessageLog.Instance.AddMessage("Not valid port!", Color.red);
            return;
        }
        IPEndPoint endpoint = new IPEndPoint(ip, port);

        string password = _password.text;

        NetworkManager._instance.JoinServer(endpoint, password);
    }

    public void ConnectExtreme()
    {
        if (!IPAddress.TryParse(_ipAddress.text, out IPAddress ip))
        {
            MessageLog.Instance.AddMessage("Not valid ip!", Color.red);
            return;
        }
        if (!int.TryParse(_port.text, out int port))
        {
            MessageLog.Instance.AddMessage("Not valid port!", Color.red);
            return;
        }
        IPEndPoint endpoint = new IPEndPoint(ip, port);

        string password = _password.text;

        if (!Guid.TryParse(_guid.text, out Guid guid)) 
        {
            MessageLog.Instance.AddMessage("Invalid guid!", Color.red);
            return;
        }

        NetworkManager._instance.AttemptHandshake(guid, endpoint, password);
    }

    public void StartLevel(int index)
    {
        if (NetworkManager._instance.IsHost)
        {
            GameManager._instance.LoadLevel(index);
        }
    }

}
