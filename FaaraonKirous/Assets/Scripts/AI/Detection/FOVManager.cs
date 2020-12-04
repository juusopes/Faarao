using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FOVManager : MonoBehaviour
{
    public static FOVManager _instance;

    public Character currentFov;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
            return;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            RaycastHit hit = RayCaster.ScreenPoint(Input.mousePosition, RayCaster.clickSelectorLayerMask);

            if (RayCaster.HitObject(hit, "ClickSelector"))
            {
                EnableGameObject(hit.collider.gameObject.GetComponentInParent<Character>());
            }
        }
    }

    public void EnableGameObject(Character character)
    {
        if (character == null)
            return;
        character.EnableFov(true);

        if(currentFov != null)
        currentFov.EnableFov(false);

        currentFov = character;
    }
}
