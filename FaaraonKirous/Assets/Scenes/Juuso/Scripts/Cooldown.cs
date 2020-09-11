using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cooldown : MonoBehaviour
{
    public float cooldownTime;
    private float nextFireTime;
    public bool isStanding = true;

    public Texture2D cursor;
    public Texture2D cursorInteract;
    public GameObject standing;
    public GameObject crouching;

    private void Start()
    {
        //standing.SetActive(true);
        crouching.SetActive(false);
    }

    private void Update()
    {
        if(Time.time > nextFireTime)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                if(isStanding)
                {
                    print("Unit is now crouching");
                    standing.SetActive(false);
                    crouching.SetActive(true);
                    isStanding = false;
                }
                else
                {
                    print("Unit is now standing");
                    standing.SetActive(true);
                    crouching.SetActive(false);
                    isStanding = true;
                }

                nextFireTime = Time.time + cooldownTime;
            }
            if(Input.GetButton("Interact"))
            {
                Cursor.SetCursor(cursorInteract, Vector2.zero, CursorMode.ForceSoftware);
            }
            else
            {
                Cursor.SetCursor(cursor, Vector2.zero, CursorMode.ForceSoftware);
            }
        }
    }
}
