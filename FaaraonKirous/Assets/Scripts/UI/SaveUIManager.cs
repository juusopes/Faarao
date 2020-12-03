using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveUIManager : MonoBehaviour
{
    public static SaveUIManager Instance { get; private set; }

    [SerializeField]
    private InputField _saveName;

    [SerializeField]
    private GameObject _saveMenu;

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
        if (_saveMenu.activeInHierarchy)
        {
            return true;
        }

        return false;
    }

    public void Save()
    {
        string saveName = _saveName.text;
        if (string.IsNullOrEmpty(saveName))
        {
            MessageLog.Instance.AddMessage("No save name provided", Color.red);
            return;
        }

        GameManager._instance.SaveToFile(saveName);

        _saveName.text = null;
    }

    public void OpenInGameMenu()
    {
        InGameMenu._instance.DeactivateMenu();
        _saveMenu.SetActive(true);
    }

    public void CloseInGameMenu()
    {
        _saveMenu.SetActive(false);
        InGameMenu._instance.ActivateMenu();
    }
}
