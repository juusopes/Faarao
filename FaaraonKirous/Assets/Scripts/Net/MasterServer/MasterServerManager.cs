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

    private DBRepository _dbRepository = new DBRepository();

    public Dictionary<int, Guid> ConnectionGuidPairs { get; private set; } = new Dictionary<int, Guid>();
    public Dictionary<Guid, int> GuidConnectionPairs { get; private set; } = new Dictionary<Guid, int>();

    public void CreateServerObject(int connection, string name, string endPoint, bool hasPassword)
    {
        ServerDB serverObject = new ServerDB(name, endPoint, hasPassword);
        ConnectionGuidPairs.Add(connection, serverObject.Id);
        GuidConnectionPairs.Add(serverObject.Id, connection);

        _dbRepository.CreateServer(serverObject);
    }

    public void RemoveServerObject(int connection)
    {
        Guid guid = ConnectionGuidPairs[connection];
        ConnectionGuidPairs.Remove(connection);
        GuidConnectionPairs.Remove(guid);

        // Delete from repository
        _dbRepository.DeleteServer(guid);
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

    private void OnApplicationQuit()
    {
        MasterServer.Instance.Stop();
    }
}

