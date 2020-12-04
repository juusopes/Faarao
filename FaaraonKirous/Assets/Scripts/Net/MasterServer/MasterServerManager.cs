using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class MasterServerManager : MonoBehaviour
{
    public static MasterServerManager Instance { get; private set; }

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
            return;
        }
    }

    private void Start()
    {
        MasterServer.Instance.Start();
    }

    private async void CreateServerObjectAsync(ServerDB server)
    {
        await _dbRepository.CreateServer(server);
    }

    private async void RemoveServerObjectAsync(Guid guid)
    {
        await _dbRepository.DeleteServer(guid);
    }

    private DBRepository _dbRepository = new DBRepository();

    public Dictionary<int, Guid> ServerIdGuidPairs { get; private set; } = new Dictionary<int, Guid>();
    public Dictionary<Guid, int> ServerGuidIdPairs { get; private set; } = new Dictionary<Guid, int>();

    public void CreateServerObject(int connection, string name, string endPoint, bool hasPassword)
    {
        ServerDB serverObject = new ServerDB(name, endPoint, hasPassword);
        ServerIdGuidPairs.Add(connection, serverObject.Id);
        ServerGuidIdPairs.Add(serverObject.Id, connection);

        // Creat to repo
        CreateServerObjectAsync(serverObject);
    }

    public void RemoveServerObject(int connection)
    {
        Guid guid = ServerIdGuidPairs[connection];
        ServerIdGuidPairs.Remove(connection);
        ServerGuidIdPairs.Remove(guid);

        // Delete from repo
        RemoveServerObjectAsync(guid);
    }

    //public void PlayerConnected(int connection)
    //{
    //    _dbRepository.DeleteServer(guid);
    //}

    //public void PlayerDisconnected(int connection)
    //{
    //    _dbRepository.DeleteServer(guid);
    //}

    public void StartHeartbeats()
    {
        StartCoroutine("SendHeartbeats");
    }

    IEnumerator SendHeartbeats()
    {
        while (true)
        {
            // Stop if masterserver becomes offline
            if (!MasterServer.Instance.IsOnline) break;

            MasterServerSend.Heartbeat();

            yield return new WaitForSeconds(Constants.masterServerHeartbeatFrequency);
        }
    }

    public void DoHandshake(int clientId, string guidString)
    {
        if (!Guid.TryParse(guidString, out Guid guid) || !ServerGuidIdPairs.ContainsKey(guid))
        {
            Debug.Log("Handshake failure");
            // TODO: send fail message
        }

        int serverId = ServerGuidIdPairs[guid];
        //string serverEndpoint = MasterServer.Instance.Connections[serverId].EndPoint.ToString();
        string clientEndpoint = MasterServer.Instance.Connections[clientId].EndPoint.ToString();
        //MasterServerSend.Handshake(clientId, serverEndpoint);
        MasterServerSend.Handshake(serverId, clientEndpoint);
    }

    private void OnApplicationQuit()
    {
        MasterServer.Instance.Stop();
    }
}

