using UnityEngine;

public class LoadUIManager : MonoBehaviour
{
    public static LoadUIManager Instance { get; private set; }

    [SerializeField]
    private MenuListCreator _saveMenuList;

    [SerializeField]
    private GameObject _loadMenu;

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
        if (_loadMenu.activeInHierarchy)
        {
            return true;
        }

        return false;
    }

    public void Start()
    {
        UpdateSaveList();
    }

    public void UpdateSaveList()
    {
        SaveUIObject[] saveList = GameManager._instance.GetSaveFiles().ToArray();

        _saveMenuList.RefreshList(saveList);
    }

    public void OpenInGameMenu()
    {
        InGameMenu._instance.DeactivateMenu();
        _loadMenu.SetActive(true);
        UpdateSaveList();
    }

    public void CloseInGameMenu()
    {
        _loadMenu.SetActive(false);
        InGameMenu._instance.ActivateMenu();
    }

    public void CloseMainMenu()
    {
        // TODO: Not implemented
    }

    public void OpenMainMenu()
    {
        // TODO: Not implemented
    }
}
