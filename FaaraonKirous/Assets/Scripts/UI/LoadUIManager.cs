using UnityEngine;

public class LoadUIManager : MonoBehaviour
{
    public static LoadUIManager Instance { get; private set; }

    [SerializeField]
    private MenuListCreator _saveMenuList;

    [SerializeField]
    private GameObject _loadMenu;

    [SerializeField]
    private MenuListCreator _saveMenuListMainMenu;

    [SerializeField]
    private GameObject _loadMenuMainMenu;

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

    public void UpdateSaveList()
    {
        SaveUIObject[] saveList = GameManager._instance.GetSaveFiles().ToArray();

        _saveMenuList.RefreshList(saveList);
    }

    public void UpdateSaveListMainMenu()
    {
        SaveUIObject[] saveList = GameManager._instance.GetSaveFiles().ToArray();

        _saveMenuListMainMenu.RefreshList(saveList);
    }

    public void OpenInGameMenu()
    {
        InGameMenu._instance.DeactivateMenu();
        _loadMenu.SetActive(true);
        UpdateSaveList();
    }

    public void Close()
    {
        _loadMenu.SetActive(false);
        _loadMenuMainMenu.SetActive(false);
    }

    public void CloseInGameMenu()
    {
        _loadMenu.SetActive(false);
        InGameMenu._instance.ActivateMenu();
    }

    public void CloseMainMenu()
    {
        _loadMenuMainMenu.SetActive(false);
        MainMenuUIManager.Instance.Open();
    }

    public void OpenMainMenu()
    {
        _loadMenuMainMenu.SetActive(true);
        UpdateSaveListMainMenu();
    }
}
