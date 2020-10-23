using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    //Which Player
    public bool playerOne;

    //Moving
    public float movementSpeed;
    public float doubleClickTimer;
    public bool isRunning;
    public bool isActiveCharacter;
    private NavMeshAgent navMeshAgent;
    private Vector3 targetV3;
    private Vector3 position;

    //PlayerSynergy
    private GameObject anotherCharacter;
    private bool lineOfSight;

    //Crouch
    public bool isCrouching;

    //Abilities
    //Indicator
    [HideInInspector]
    public bool ability1Active;
    public GameObject indicator;
    private GameObject line;
    [HideInInspector]
    public GameObject visibleInd;

    //Invisibility
    public bool isInvisible;

    //Camera
    private GameObject camControl;

    //Interactive
    public GameObject interactObject;

    //Climbing
    public GameObject climbObject;
    private GameObject savedClimbable;
    private bool climbing;
    private bool climbSuccess;
    public bool grounded;

    //Dying
    private DeathScript death;

    //Attack
    public GameObject targetEnemy;

    //Menu
    public LevelController lC;
    public InGameMenu menu;

    public bool IsDead => death.isDead;

    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsDead)
        {
            Moving();
            LineOfSight();
            KeyControls();
            Invisibility();
            SetIndicator();
            Climb();
        }
    }

    private void Initialize()
    {
        navMeshAgent = this.GetComponent<NavMeshAgent>();
        camControl = GameObject.FindGameObjectWithTag("MainCamera").transform.parent.gameObject;

        GameObject[] tempCharacters = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject tempCharacter in tempCharacters)
        {
            if (tempCharacter != this.gameObject)
            {
                anotherCharacter = tempCharacter;
            }
        }
        climbing = false;
        lC = GameObject.FindGameObjectWithTag("LevelController").GetComponent<LevelController>();
        menu = lC.canvas;
        death = GetComponent<DeathScript>();
        targetV3 = transform.position;
        Stay();
    }
    public void Moving()
    {
        if (menu.menuActive)
        {
            Stay();
        } else { 
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit = new RaycastHit();
            //DoubleClick Check
            if (isActiveCharacter)
            {
                if (Input.GetKeyDown(KeyCode.Mouse0) && !PointerOverUI())
                {
              
                    if (Physics.Raycast(ray, out hit))
                    {
                        targetV3 = hit.point;
                    }
                    if (isRunning)
                    {
                        isRunning = false;
                    }
                    if (doubleClickTimer < 0.5f)
                    {
                        isRunning = true;
                    }
                    if (doubleClickTimer >= 0.5f)
                    {
                        doubleClickTimer = 0;
                    }
                }
            }
            if (doubleClickTimer < 0.5f)
            {
                doubleClickTimer += Time.deltaTime;
            }
            //Moving
            if ((!playerOne && !GetComponent<PriestAbilities>().useTeleknesis) || playerOne)
            {
                if (isRunning)
                {
                    navMeshAgent.speed = movementSpeed * 1.5f;
                }
                else if (isCrouching)
                {
                    navMeshAgent.speed = movementSpeed * 0.5f;
                }
                else
                {
                    navMeshAgent.speed = movementSpeed;
                }
                if (!climbing)
                {
                    navMeshAgent.SetDestination(targetV3);
                }
                if (position == transform.position)
                {
                    isRunning = false;
                }
                else
                {
                    position = transform.position;
                }
            }
        }
    }
    public void Stay()
    {
        navMeshAgent.SetDestination(transform.position);
        targetV3 = transform.position;
    }

    private void LineOfSight()
    {
        RaycastHit hit;
        Vector3 fromPosition = transform.position;
        Vector3 toPosition = anotherCharacter.transform.position;
        Vector3 direction = toPosition - fromPosition;

        Debug.DrawRay(fromPosition, direction);

        if (Physics.Raycast(transform.position, direction, out hit))
        {
            if (hit.collider.tag != "Player")
            {
                lineOfSight = false;
            }
        }
        else
        {
            lineOfSight = true;
        }
    }
    public void Crouch()
    {
        if (isActiveCharacter)
        {
            if (isRunning)
            {
                isRunning = false;
                isCrouching = true;
            }
            else if (!isCrouching)
            {
                isCrouching = true;
            }
            else
            {
                isCrouching = false;
            }
        }
    }
    //private void InvisiblitySpell()
    //{
    //    if (isActiveCharacter)
    //    {
    //        if (lineOfSight)
    //        {
    //            anotherCharacter.GetComponent<PlayerController>().isInvisible = true;
    //        }
    //    }
    //}

    private void Invisibility()
    {
        if (isInvisible)
        {
            this.gameObject.tag = "PlayerInvisible";
        }
        else
        {
            this.gameObject.tag = "Player";
        }
    }

    private void SetIndicator()
    {
        if (isActiveCharacter)
        {
            if (ability1Active)
            {
                if (visibleInd == null)
                {
                    visibleInd = Instantiate(indicator);
                    line = visibleInd.transform.GetChild(0).gameObject;
                    visibleInd.GetComponent<AbilityIndicator>().player = this.gameObject;
                }
                else
                {
                    visibleInd.SetActive(true);
                    line.SetActive(true);
                }
            }
            else
            {
                if (visibleInd != null)
                {
                    visibleInd.SetActive(false);
                    line.SetActive(false);
                }
            }
        }
        else
        {
            if (visibleInd != null)
            {
                visibleInd.SetActive(false);
                line.SetActive(false);
            }
        }
    }

    private void CamFollow()
    {
        if (isActiveCharacter)
        {
            if (camControl.GetComponent<CameraControl>().camFollow)
            {
                camControl.GetComponent<CameraControl>().camFollow = false;
                camControl.transform.parent = null;
            }
            else
            {
                camControl.GetComponent<CameraControl>().camFollow = true;
                camControl.transform.parent = this.gameObject.transform;
            }
        }
    }

    public void Interact()
    {
        if (interactObject != null)
        {
            if (!interactObject.GetComponent<Activator>().activated)
            {
                interactObject.GetComponent<Activator>().activated = true;
            }
            else
            {
                interactObject.GetComponent<Activator>().activated = false;
            }
        }
    }
    public void Climb()
    {
        if (climbObject != null && climbing)
        {
            Vector3 rot = Vector3.RotateTowards(transform.position, climbObject.transform.position, 360, 360);
            transform.rotation = Quaternion.LookRotation(rot);
            Debug.Log(climbing);
            climbSuccess = true;
            float yAxisValue = 0.01f;
            GetComponent<Rigidbody>().isKinematic = true;
            gameObject.GetComponent<NavMeshAgent>().isStopped = true;
            GetComponent<NavMeshAgent>().enabled = false;
            transform.Translate(new Vector3(0, yAxisValue, 0));
            savedClimbable = climbObject;
        } else if(climbSuccess)
        {
            transform.Translate(new Vector3(0, 0, 0.01f));
            if (grounded)
            {
                climbSuccess = false;
            }
        } else
        {
            GetComponent<NavMeshAgent>().enabled = true;
            gameObject.GetComponent<NavMeshAgent>().isStopped = false;
        }
    }

    public void Attack()
    {
        if (targetEnemy != null)
        {
            targetEnemy.GetComponent<DeathScript>().damage = 1;
        }
    }

    public void UseAbility1()
    {
        if (!ability1Active)
        {
            ability1Active = true;
        }
        else
        {
            ability1Active = false;
        }
    }

    private void KeyControls()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Crouch();
        }
        //if (Input.GetKeyDown(KeyCode.I))
        //{
        //    InvisiblitySpell();
        //}
        if (Input.GetKeyDown(KeyCode.A))
        {
            Attack();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            UseAbility1();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            CamFollow();
        }
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            Interact();
        }
        if (Input.GetKeyDown(KeyCode.U))
        {
            if (climbing)
            {
                climbing = false;
            }
            else
            {
                climbing = true;
            }
        }
    }
    public bool PointerOverUI()
    {
        return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }
}

