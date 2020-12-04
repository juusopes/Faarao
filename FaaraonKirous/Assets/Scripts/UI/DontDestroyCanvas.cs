using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DontDestroyCanvas : MonoBehaviour
{
    public static DontDestroyCanvas Instance { get; private set; } = null;

    private LoadUIManager _loadUiManager;
    private SaveUIManager _saveUiManager;
    private HostUIManager _hostUiManager;

    public GameObject loadingScreen;
    public Slider loadingScreenSlider;

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
        if (_loadUiManager.IsOpen() || _saveUiManager.IsOpen() || _hostUiManager.IsOpen() || IsOpenLoadingScreen())
        {
            return true;
        }

        return false;
    }

    public bool IsOpenLoadingScreen()
    {
        return loadingScreen.activeInHierarchy;
    }
}
