using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseMenuObject : MonoBehaviour
{
    public Image panelBackground;
    public Text nameText;
    public virtual void UpdateObject(int index, object inObj) { }
}
