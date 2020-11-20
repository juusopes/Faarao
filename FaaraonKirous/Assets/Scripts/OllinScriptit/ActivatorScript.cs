using System.Collections;
using System.Collections.Generic;
//using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class ActivatorScript : MonoBehaviour
{
    public bool activated;
    public void Activate()
    {
        if (activated)
        {
            activated = false;
        }
        else
        {
            activated = true;
        }
    }
    private void ShootRay()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit = new RaycastHit();
        //DoubleClick Check
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, RayCaster.attackLayerMask))
        {
        }
    }
}
