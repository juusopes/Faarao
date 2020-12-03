using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HostUIManager : MonoBehaviour
{
    public static HostUIManager Instance { get; private set; }

    [SerializeField]
    private InputField _sessionName;

    [SerializeField]
    private InputField _sessionPassword;

    [SerializeField]
    private GameObject _hostMenu;

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

    public bool IsOpen()
    {
        if (_hostMenu.activeInHierarchy)
        {
            return true;
        }

        return false;
    }

    public void Host()
    {
        // Session name
        string sessionName = _sessionName.text;
        if (string.IsNullOrEmpty(sessionName))
        {
            MessageLog.Instance.AddMessage("No session name provided", Color.red);
            return;
        }

        // Password
        string password = _sessionPassword.text;

        // TODO: Provide session name and password to host server
        if (NetworkManager._instance.HostServer())
        {
            CloseInGameMenu();
        }
        else
        {
            return;
        }
    }

    public void OpenInGameMenu()
    {
        InGameMenu._instance.DeactivateMenu();
        _hostMenu.SetActive(true);
    }

    public void CloseInGameMenu()
    {
        _hostMenu.SetActive(false);
        InGameMenu._instance.ActivateMenu();
    }
}
