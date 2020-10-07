using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HenkkaPlayerContainer : MonoBehaviour
{
    private static HenkkaPlayerContainer _instance;

    public static HenkkaPlayerContainer Instance { get { return _instance; } }
    private static GameObject[] players;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    private void Start()
    {
        UpdatePlayerReferences();
    }

    private void UpdatePlayerReferences()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        if(players.Length == 0)
        {
            Debug.LogError("No players found!");
        }
    }

    public GameObject[] GetPlayerReferences()
    {
        return players;
    }
}


