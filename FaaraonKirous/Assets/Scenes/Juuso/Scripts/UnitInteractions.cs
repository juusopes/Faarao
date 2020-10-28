using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitInteractions : MonoBehaviour
{
    public float cooldownTime;
    public float nextFireTime;
    public bool isStanding = true;
    public bool onCooldown = false;

    public Texture2D cursor, cursorInteract, cursorAttack;
    public bool checkbox;
    public GameObject standing, crouching;
    public Image imageCooldown;

    private void Start()
    {
        crouching.SetActive(false);

    }

    private void Update()
    {
        bool menuActivated = GetComponent<InGameMenu>().menuActive;

        // cannot use abilities etc. if menu is active
        if (menuActivated == false)
        {
            if (Input.GetButtonDown("Stance"))
            {
                if (isStanding)
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
            }

            if (Input.GetButton("Attack"))
            {
                Cursor.SetCursor(cursorAttack, Vector2.zero, CursorMode.ForceSoftware);
            }
            else if (Input.GetButton("Interact"))
            {
                Cursor.SetCursor(cursorInteract, Vector2.zero, CursorMode.ForceSoftware);
            }
            else
            {
                Cursor.SetCursor(cursor, Vector2.zero, CursorMode.ForceSoftware);
            }

            if (Time.time > nextFireTime)
            {
                onCooldown = false;
                imageCooldown.fillAmount = 1;

                if (Input.GetButtonDown("Ability"))
                {
                    cooldownTime = 5;
                    onCooldown = true;

                    nextFireTime = Time.time + cooldownTime;
                }
            }

            if (onCooldown)
            {
                imageCooldown.fillAmount -= 1 / cooldownTime * Time.deltaTime;
            }
            if (!onCooldown)
            {
                imageCooldown.fillAmount = 0;
            }
        }
    }
}
