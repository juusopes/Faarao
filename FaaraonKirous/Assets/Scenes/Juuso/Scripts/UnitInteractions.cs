using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitInteractions : MonoBehaviour
{
    public static UnitInteractions _instance;

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

    public float cooldownTime;
    public float nextFireTime;
    public bool isStanding = true;
    public bool isStanding2 = true;
    public bool onCooldown = false;

    public bool checkbox;

    [HideInInspector]
    public GameObject standing, crouching;
    [HideInInspector]
    public GameObject standing2, crouching2;

    public GameObject character1, character2;
    [HideInInspector]
    public GameObject skillGroup1, skillGroup2;
    [HideInInspector]
    public GameObject generalSkillGroup;

    [HideInInspector]
    public GameObject skillR1, skillQ1, skillW1, skillE1;
    [HideInInspector]
    public GameObject skillQ2, skillW2, skillE2;

    //public GameManager currentCharacter;
    public PlayerController ActiveAbilities;
    public PlayerController ActiveAbilities2;

    public int activeCharacter;
    public GameObject deathCanvas1, deathCanvas2;

    public GameObject gameOverMenu;
    [HideInInspector]
    public bool gameOver;

    private void Start()
    {
    }

    private void Update()
    {
        if (GameManager._instance.IsFullyLoaded && !DontDestroyCanvas.Instance.IsOpen())
        {
            AllowedAbilities();
            GameOverCheck();
        }
    }

    public void AllowedAbilities()
    {
        bool menuActivated = GetComponent<InGameMenu>().menuActive;

        int allowedAbilities = 0;

        // cannot use abilities etc. if menu is active
        if (!menuActivated)
        {
            for (int i = 0; i < 11; i++)
            {
                bool v = GameManager._instance.Pharaoh.GetComponent<PlayerController>().abilityAllowed[i];
                bool d = GameManager._instance.Pharaoh.GetComponent<PlayerController>().IsDead;
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

                if (d && activeCharacter == 1)
                {
                    if (!gameOver)
                    {
                        deathCanvas1.SetActive(true);
                    }
                }
                else if(!d || activeCharacter != 1)
                {
                    if (!gameOver)
                    {
                        deathCanvas1.SetActive(false);
                    }
                }
            }

            allowedAbilities = 0;

            for (int i = 0; i < 11; i++)
            {
                bool v = GameManager._instance.Priest.GetComponent<PlayerController>().abilityAllowed[i];
                bool d = GameManager._instance.Priest.GetComponent<PlayerController>().IsDead;
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

                if (d && activeCharacter == 2)
                {
                    if (!gameOver)
                    {
                        deathCanvas2.SetActive(true);
                    }
                }
                else if (!d || activeCharacter != 2)
                {
                    if (!gameOver)
                    {
                        deathCanvas2.SetActive(false);
                    }
                }
            }
            allowedAbilities = 0;
        }
    }

    public void StanceCheckUI()
    {
        if (isStanding && activeCharacter == 1)
        {
            standing.SetActive(false);
            crouching.SetActive(true);
            isStanding = false;
        }
        else if (!isStanding && activeCharacter == 1)
        {
            standing.SetActive(true);
            crouching.SetActive(false);
            isStanding = true;
        }

        if (isStanding2 && activeCharacter == 2)
        {
            standing2.SetActive(false);
            crouching2.SetActive(true);
            isStanding2 = false;
        }
        else if (!isStanding2 && activeCharacter == 2)
        {
            standing2.SetActive(true);
            crouching2.SetActive(false);
            isStanding2 = true;
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

    public void GameOverCheck()
    {
        if (ActiveAbilities.IsDead && ActiveAbilities2.IsDead)
        {
            if(!this.gameObject.GetComponent<InGameMenu>().menuActive)
            {
                gameOverMenu.SetActive(true);
                gameOver = true;
            } else
            {
                gameOverMenu.SetActive(false);
            }
        } else
        {
                gameOverMenu.SetActive(false);
                gameOver = false;
        }
    }
}