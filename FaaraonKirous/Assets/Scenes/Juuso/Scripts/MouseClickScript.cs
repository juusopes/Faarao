using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseClickScript : MonoBehaviour
{
    public GameObject mouseEffect;
    private GameObject instantiatedMouseClick;
    private float mouseClickTimer = 0;
    // Start is called before the first frame update
    void Start()
    {
        instantiatedMouseClick = Instantiate(mouseEffect);
        instantiatedMouseClick.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        ClickEffectTime();
     if (Input.GetKeyDown(KeyCode.Mouse1) && !PointerOverUI())
        {
            PlayClick();
        }   
    }

    private void PlayClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, RayCaster.attackLayerMask))
        {
            mouseEffect.SetActive(false);
            mouseClickTimer = 0.2f;
            instantiatedMouseClick.transform.position = hit.point;
        }
        Debug.Log("Click Played");
    }

    private void ClickEffectTime()
    {
        if (mouseClickTimer > 0)
        {
            instantiatedMouseClick.SetActive(true);
            mouseClickTimer -= Time.deltaTime;
        } else
        {
            mouseClickTimer = 0;
            instantiatedMouseClick.SetActive(false);
        }
        Debug.Log("Click Timer");
    }
    public bool PointerOverUI()
    {
        if (UnityEngine.EventSystems.EventSystem.current == null)
            return false;
        //return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
        return UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject != null;
    }
}
