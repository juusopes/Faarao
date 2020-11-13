using System.Collections;
using System.Collections.Generic;
//using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class ActivatorScript : MonoBehaviour
{
    public bool activated;
    public void Activate()
    {
        if(activated)
        {
            activated = false;
        } else
        {
            activated = true;
        }
    }
}
