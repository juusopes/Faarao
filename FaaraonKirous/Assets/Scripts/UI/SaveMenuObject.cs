using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveMenuObject : BaseMenuObject
{
    public Text date;
    private SaveUIObject saveObject;

    public override void UpdateObject(int index, object inObj)
    {
        SaveUIObject save = (SaveUIObject)inObj;
        if (save == null) return;

        saveObject = save;
        nameText.text = save.Name.Substring(0, save.Name.LastIndexOf("."));
        date.text = save.CreationDate.ToString();
    }

    public void Delete()
    {
        GameManager._instance.DeleteFile(saveObject.Name);
        LoadUIManager.Instance.UpdateSaveListAll();
    }

    public void Load()
    {
        if (GameManager._instance.LoadFromFile(saveObject.Name))
        {
            LoadUIManager.Instance.Close();
        }
    }
}
