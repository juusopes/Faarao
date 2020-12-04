using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using UnityEngine;

public class ServersUI : MonoBehaviour
{
    public static ServersUI Instance { get; private set; }

    [SerializeField]
    private MenuListCreator _serverMenuList;

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

    private HttpClient _httpClient = new HttpClient();

    public async void UpdateServerList()
    {
        string request = $"{Constants.apiAddress}api/servers/";

        var response = await _httpClient.GetAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();

        List<ServerDB> serverObjs = JsonConvert.DeserializeObject<List<ServerDB>>(responseString);

        List<ServerUIObject> uiObjects = new List<ServerUIObject>();

        foreach (ServerDB server in serverObjs)
        {
            ServerUIObject newUIObject = new ServerUIObject();
            newUIObject.Name = server.Name;
            newUIObject.EndPoint = server.EndPoint;
            newUIObject.HasPassword = server.HasPassword;
            newUIObject.Guid = server.Id;
        }

        ServerUIObject[] serverList = uiObjects.ToArray();

        _serverMenuList.RefreshList(serverList);
    }
}

