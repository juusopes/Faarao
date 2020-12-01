using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private InputField _nameField;

    public void Start()
    {
        string name = GameManager._instance.GetName();
        if (name != null)
        {
            _nameField.text = name;
        }
    }

    public void SaveName(string name)
    {
        GameManager._instance.SetName(name);
    }
}
