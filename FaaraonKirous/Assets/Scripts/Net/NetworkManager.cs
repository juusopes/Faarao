using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager _instance;

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

    public void HostServer()
    {
        if (Client.Instance.IsOnline || Server.Instance.IsOnline)
        {
            Server.Instance.Start(26950);
        }
        else
        {
            Debug.Log("Cannot create server if server or client is online!");
        }
    }

    public void JoinServer()
    {
        if (Client.Instance.IsOnline || Server.Instance.IsOnline)
        {
            // TODO: Server IP and port should be given through text fields
            string serverIp = "127.0.0.1";
            int serverPort = 26950;
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(serverIp), serverPort);
            Client.Instance.ConnectToServer(ipEndPoint);
        }
        else
        {
            Debug.Log("Cannot join server if server or client is online!");
        }
    }

    private void OnApplicationQuit()
    {
        Server.Instance.Stop();
        Client.Instance.Disconnect();
    }
}
