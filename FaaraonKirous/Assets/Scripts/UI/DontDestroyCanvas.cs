using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroyCanvas : MonoBehaviour
{
    public static DontDestroyCanvas Instance { get; private set; } = null;

    private LoadUIManager _loadUiManager;
    private SaveUIManager _saveUiManager;
    private HostUIManager _hostUiManager;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(gameObject);
            return;
        }

        _loadUiManager = GetComponent<LoadUIManager>();
        _saveUiManager = GetComponent<SaveUIManager>();
        _hostUiManager = GetComponent<HostUIManager>();
    }

    public bool IsOpen()
    {
        if (_loadUiManager.IsOpen() || _saveUiManager.IsOpen() || _hostUiManager.IsOpen())
        {
            return true;
        }

        return false;
    }
}
