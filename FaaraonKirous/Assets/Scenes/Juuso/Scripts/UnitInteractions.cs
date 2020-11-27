using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitInteractions : MonoBehaviour
{
    public float cooldownTime;
    public float nextFireTime;
    public bool isStanding = true;
    public bool isStanding2 = true;
    public bool onCooldown = false;

    //public Texture2D cursor, cursorInteract, cursorAttack;
    public bool checkbox;
    public GameObject standing, crouching;
    public GameObject standing2, crouching2;

    //public Image imageCooldown;

    public GameObject character1, character2;
    public GameObject skillGroup1, skillGroup2;
    public GameObject generalSkillGroup;

    [HideInInspector]
    public GameObject skillR1, skillQ1, skillW1, skillE1;
    [HideInInspector]
    public GameObject skillQ2, skillW2, skillE2;

    public DeathScript isDead1, isDead2;

    //public GameManager currentCharacter;
    public PlayerController ActiveAbilities;
    public PlayerController ActiveAbilities2;

    public int activeCharacter;



    private void Start()
    {
        crouching.SetActive(false);

        // TODO
        activeCharacter = 1;
    }

    private void Update()
    {
        AllowedAbilities();
        //UnselectCharacter();
    }

    public void AllowedAbilities()
    {
        bool menuActivated = GetComponent<InGameMenu>().menuActive;

        int allowedAbilities = 0;

        bool isDead11 = isDead1.isDead;
        bool isDead22 = isDead2.isDead;

        // cannot use abilities etc. if menu is active
        if (!menuActivated)
        {
            for (int i = 0; i < 11; i++)
            {
                bool v = GameManager._instance.Pharaoh.GetComponent<PlayerController>().abilityAllowed[i];
                if (v)
                {
                    if (allowedAbilities == 10)
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
                    if (allowedAbilities == 2)
                    {
                        skillE1.SetActive(true);
                    }
                }
                allowedAbilities++;
            }

            allowedAbilities = 0;

            for (int i = 0; i < 11; i++)
            {
                bool v = GameManager._instance.Priest.GetComponent<PlayerController>().abilityAllowed[i];
                if (v)
                {
                    if (allowedAbilities == 1)
                    {
                        skillQ2.SetActive(true);
                    }
                    if (allowedAbilities == 3)
                    {
                        skillW2.SetActive(true);
                    }
                    if (allowedAbilities == 2)
                    {
                        skillE2.SetActive(true);
                    }
                }
                allowedAbilities++;
            }
            allowedAbilities = 0;

            if (Input.GetButtonDown("Stance"))
            {
                if (isStanding && activeCharacter == 1 && !isDead11)
                {
                    standing.SetActive(false);
                    crouching.SetActive(true);
                    isStanding = false;
                }
                else if (!isStanding && activeCharacter == 1 && !isDead11)
                {
                    standing.SetActive(true);
                    crouching.SetActive(false);
                    isStanding = true;
                }

                if (isStanding2 && activeCharacter == 2 && !isDead22)
                {
                    standing2.SetActive(false);
                    crouching2.SetActive(true);
                    isStanding2 = false;
                }
                else if (!isStanding2 && activeCharacter == 2 && !isDead22)
                {
                    standing2.SetActive(true);
                    crouching2.SetActive(false);
                    isStanding2 = true;
                }
            }
        }
    }

    public void SelectPharaohUI()
    {
            activeCharacter = 1;

            generalSkillGroup.SetActive(true);

            character1.SetActive(true);
            character2.SetActive(false);

            skillGroup1.SetActive(true);
            skillGroup2.SetActive(false);
    }

    public void SelectPriestUI()
    {
        activeCharacter = 2;

        generalSkillGroup.SetActive(true);

        character1.SetActive(false);
        character2.SetActive(true);

        skillGroup1.SetActive(false);
        skillGroup2.SetActive(true);
    }

    public void UnselectCharacter()
    {
        activeCharacter = 0;

        character1.SetActive(false);
        character2.SetActive(false);

        skillGroup1.SetActive(false);
        skillGroup2.SetActive(false);
        generalSkillGroup.SetActive(false);

    }
}