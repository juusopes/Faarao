using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroyCanvas : MonoBehaviour
{
    public static DontDestroyCanvas Instance { get; private set; } = null;

    private LoadUIManager _loadUiManager;
    private SaveUIManager _saveUiManager;

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
    }

    public bool IsOpen()
    {
        if (_loadUiManager.IsOpen() || _saveUiManager.IsOpen())
        {
            return true;
        }

        return false;
    }
}
