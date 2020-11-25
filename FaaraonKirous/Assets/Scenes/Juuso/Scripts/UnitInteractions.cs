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

    //public Texture2D cursor, cursorInteract, cursorAttack;
    public bool checkbox;
    public GameObject standing, crouching;
    //public Image imageCooldown;

    public GameObject character1, character2;
    public GameObject skillGroup1, skillGroup2;

    [HideInInspector]
    public GameObject skillR1, skillQ1, skillW1, skillE1;
    [HideInInspector]
    public GameObject skillQ2, skillW2, skillE2;

    public DeathScript isDead1, isDead2;

    //public GameManager currentCharacter;
    public PlayerController ActiveAbilities;
    public PlayerController ActiveAbilities2;



    private void Start()
    {
        crouching.SetActive(false);
    }

    private void Update()
    {
        bool menuActivated = GetComponent<InGameMenu>().menuActive;
        //bool allowedAbilities = GetComponent<PlayerController>().abilityAllowed[];

        int allowedAbilities = 0;
        for (int i = 0; i < 11; i++)
        {
            //bool v = GameObject.Find("Pharaoh").GetComponent<PlayerController>().abilityAllowed[i];
            bool v = GameManager._instance.Pharaoh.GetComponent<PlayerController>().abilityAllowed[i];
            if (v)
            {
                //print(allowedAbilities);
                if(allowedAbilities == 10)
                {
                    skillR1.SetActive(true);
                }
                if (allowedAbilities == 1)
                {
                    skillQ1.SetActive(true);
                }
                if (allowedAbilities == 3)
                {
                    skillW1.SetActive(true);
                }
                if (allowedAbilities == 5)
                {
                    skillE1.SetActive(true);
                }
            }
            allowedAbilities++;
        }

        allowedAbilities = 0;

        for (int i = 0; i < 11; i++)
        {
            //bool v = GameObject.Find("Priest").GetComponent<PlayerController>().abilityAllowed[i];
            bool v = GameManager._instance.Priest.GetComponent<PlayerController>().abilityAllowed[i];
            if (v)
            {
                //print(allowedAbilities);
                if (allowedAbilities == 1)
                {
                    skillQ2.SetActive(true);
                }
                if (allowedAbilities == 2)
                {
                    skillW2.SetActive(true);
                }
                if (allowedAbilities == 3)
                {
                    skillE2.SetActive(true);
                }
            }
            allowedAbilities++;
        }



        //for(each item in the list)
        //{
        //    if (item[i])
        //        trueItem = i;
        //}

        //for(each item in the list)
        //{
        //    if (i != trueItem)
        //        item[i] = false;
        //}

        //int currentCharacter2 = GetComponent<PlayerController>().abilityAllowed[];
        //print(currentCharacter2 + " is currently controlled");

        bool isDead11 = isDead1.isDead;
        bool isDead22 = isDead2.isDead;

        //if(isDead11)
        //{
        //    skillGroup1.SetActive(false);
        //}
        //if (isDead22)
        //{
        //    skillGroup2.SetActive(false);
        //}

        //print("pharaoh is dead = " + isDead11);
        //print("priest is dead = " + isDead22);

        // cannot use abilities etc. if menu is active
        if (!menuActivated)
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

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                character1.SetActive(true);
                character2.SetActive(false);

                if (!isDead11)
                {
                    skillGroup1.SetActive(true);
                }
                skillGroup2.SetActive(false);
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                character1.SetActive(false);
                character2.SetActive(true);

                if (!isDead22)
                {
                    skillGroup2.SetActive(true);
                }
                skillGroup1.SetActive(false);
            }
        }


        //if (Input.GetButton("Attack"))
        //{
        //    Cursor.SetCursor(cursorAttack, Vector2.zero, CursorMode.ForceSoftware);
        //}
        //else if (Input.GetButton("Interact"))
        //{
        //    Cursor.SetCursor(cursorInteract, Vector2.zero, CursorMode.ForceSoftware);
        //}
        //else
        //{
        //    Cursor.SetCursor(cursor, Vector2.zero, CursorMode.ForceSoftware);
        //}

        //if (Time.time > nextFireTime)
        //{
        //    onCooldown = false;
        //    imageCooldown.fillAmount = 1;

        //    if (Input.GetButtonDown("Ability"))
        //    {
        //        cooldownTime = 5;
        //        onCooldown = true;

        //        nextFireTime = Time.time + cooldownTime;
        //    }
        //}

        //if (onCooldown)
        //{
        //    imageCooldown.fillAmount -= 1 / cooldownTime * Time.deltaTime;
        //}
        //if (!onCooldown)
        //{
        //    imageCooldown.fillAmount = 0;
        //}
    }
}
