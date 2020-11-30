using System.Collections;
using System.Collections.Generic;
//using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class ActivatorScript : MonoBehaviour
{
    private ActivatableObjectManager _objectManager;

    public void Awake()
    {
        _objectManager = GetComponent<ActivatableObjectManager>();
    }


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

        if (NetworkManager._instance.ShouldSendToClient)
        {
            ServerSend.ActivationStateChanged(_objectManager.Id, activated);
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
