using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    // Managers
    private PlayerNetManager PlayerNetManager { get; set; }


    //Which Player
    public bool playerOne;

    //Moving
    public float movementSpeed;
    public float doubleClickTimer;
    public bool IsCurrentPlayer { get; set; } = false;
    public bool IsActivePlayer { get; set; } = false;
    private NavMeshAgent navMeshAgent;
    private Vector3 targetV3;
    private Vector3 position;

    //Running
    [HideInInspector]
    public bool _isRunning;
    public bool IsRunning 
    { 
        get
        {
            return _isRunning;
        } 
        set
        {
            _isRunning = value;
            if (NetworkManager._instance.ShouldSendToServer)
            {
                // TODO: Is running packet to client
            }
            else if (NetworkManager._instance.ShouldSendToClient)
            {
                // TODO: Is running packet to server
            }
        }
    }

    //PlayerSynergy
    private GameObject anotherCharacter;
    private bool lineOfSight;

    //Crouch
    [HideInInspector]
    public bool _isCrouching;
    public bool IsCrouching
    {
        get
        {
            return _isCrouching;
        }
        set
        {
            _isCrouching = value;
            if (NetworkManager._instance.ShouldSendToServer)
            {
                // TODO: Is crouching packet to client
            }
            else if (NetworkManager._instance.ShouldSendToClient)
            {
                // TODO: Is crouching packet to server
            }
        }
    }

    //Abilities
    //Indicator
    //[HideInInspector]
    public bool abilityActive;
    public GameObject indicator;
    private GameObject line;
    [HideInInspector]
    public GameObject visibleInd;
    public int abilityNum;
    public bool inRange;
    //Invisibility
    public bool isInvisible;

    //Camera
    private GameObject camControl;

    //Interactive
    public GameObject interactObject;

    //Climbing
    //public GameObject climbObject;
    //private GameObject savedClimbable;
    //private bool climbing;
    //private bool climbSuccess;
    //public bool grounded;
    private OffMeshLinkMovement linkMovement;       //new create

    //Dying
    private DeathScript death;

    public bool IsDead => death.isDead;

    //Attack
    public GameObject targetEnemy;
    private GameObject target;
    private bool useAttack;

    //Menu
    public LevelController lC;
    public InGameMenu menu;


    private void Awake()
    {
        Initialize();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsDead)
        {
            Moving();
            LineOfSight();
            KeyControls();

            
            Invisibility();  // TODO: Does not work in multiplayer

            if (NetworkManager._instance.IsHost)
            {
                TestOffLink();
            }
            
            Attack();
            //Climb();
        }
        else
        {
            if (NetworkManager._instance.IsHost)
            {
                StopNavigation();
            }
            abilityActive = false;
        }
        SetIndicator();
    }
    private void Initialize()
    {
        PlayerNetManager = GetComponent<PlayerNetManager>();
        death = GetComponent<DeathScript>();
        navMeshAgent = this.GetComponent<NavMeshAgent>();
        camControl = GameObject.FindGameObjectWithTag("MainCamera").transform.parent.gameObject;
        lC = GameObject.FindGameObjectWithTag("LevelController").GetComponent<LevelController>();
        menu = lC.canvas;

        if (NetworkManager._instance.IsHost)
        {
            linkMovement = new OffMeshLinkMovement(transform, navMeshAgent, 0.5f, 1f);
            targetV3 = transform.position;
            Stay();
        }
        else
        {
            Destroy(navMeshAgent);
            Destroy(death);
        }

        GameObject[] tempCharacters = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject tempCharacter in tempCharacters)
        {
            if (tempCharacter != this.gameObject)
            {
                anotherCharacter = tempCharacter;
            }
        }

        abilityActive = false;
    }
    public void Moving()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit = new RaycastHit();
        //DoubleClick Check
        if (IsCurrentPlayer)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0) && !PointerOverUI())
            {
                if (Physics.Raycast(ray, out hit))
                {
                    targetV3 = hit.point;
                }
                if (IsRunning)
                {
                    IsRunning = false;
                }
                if (doubleClickTimer < 0.5f)
                {
                    IsRunning = true;
                }
                if (doubleClickTimer >= 0.5f)
                {
                    doubleClickTimer = 0;
                }

                if (NetworkManager._instance.ShouldSendToServer)
                {
                    ClientSend.SetDestinationRequest(PlayerNetManager.Type, targetV3);
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
            if (NetworkManager._instance.IsHost)
            {
                if (IsRunning)
                {
                    navMeshAgent.speed = movementSpeed * 1.5f;
                }
                else if (IsCrouching)
                {
                    navMeshAgent.speed = movementSpeed * 0.5f;
                }
                else
                {
                    navMeshAgent.speed = movementSpeed;
                }
                //if (!climbing)
                //{
                SetDestination(targetV3);
                //}

                if (position == transform.position)
                {
                    IsRunning = false;
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
        SetDestination(transform.position);
        targetV3 = transform.position;
    }
    
    public void GiveDestination(Vector3 v3)
    {
        targetV3 = v3;
        if (NetworkManager._instance.IsHost)
        {
            SetDestination(targetV3);
        }
        else
        {
            if (NetworkManager._instance.ShouldSendToServer)
            {
                ClientSend.SetDestinationRequest(PlayerNetManager.Type, targetV3);
            }
        }
    }

    public Vector3 GetPosition()
    {
        return transform.position;
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
        if (IsCurrentPlayer)
        {
            if (IsRunning)
            {
                IsRunning = false;
                IsCrouching = true;
            }
            else if (!IsCrouching)
            {
                IsCrouching = true;
            }
            else
            {
                IsCrouching = false;
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
        if (IsCurrentPlayer)
        {
            if (abilityActive)
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
        if (IsCurrentPlayer)
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
        // TODO: Doesn't work in multiplayer
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

    public void Attack()
    {
        if (NetworkManager._instance.IsHost)
        {
            if (abilityNum == 9)
            {
                if (lC.targetObject != null)
                {
                    target = lC.targetObject;
                }
                else if (!useAttack)
                {
                    target = null;
                }
                if (target != null)
                {
                    if (Input.GetKeyDown(KeyCode.Mouse1) && IsCurrentPlayer)
                    {
                        targetV3 = target.transform.position;
                        navMeshAgent.SetDestination(targetV3);

                        useAttack = true;
                        abilityActive = false;
                        GetComponent<PlayerController>().visibleInd.GetComponent<AbilityIndicator>().targetTag = "Enemy";
                    }
                    if (targetEnemy == target)
                    {
                        targetEnemy.GetComponent<DeathScript>().damage = 1;
                        targetEnemy = null;
                        target = null;
                        useAttack = false;
                        abilityNum = 0;
                        Stay();
                    }
                }
            }
        }
    }

    public void UseAbility(int tempAbilityNum)
    {
        if (!abilityActive)
        {
            abilityActive = true;
            abilityNum = tempAbilityNum;
        }
        else if (abilityNum != tempAbilityNum)
        {
            abilityActive = true;
            abilityNum = tempAbilityNum;
        }
        else
        {
            abilityActive = false;
            abilityNum = 0;
        }

    }
    private void TestOffLink()
    {
        if (navMeshAgent.isOnOffMeshLink && linkMovement.CanStartLink())
        {
            OffMeshLinkRoute route = linkMovement.GetOffMeshLinkRoute();
            StartCoroutine(linkMovement.MoveAcrossNavMeshLink(route));
        }
    }

    public void SetDestination(Vector3 position, bool changeTarget = false)
    {
        if (NetworkManager._instance.IsHost)
        {
            if (changeTarget) targetV3 = position;

            if (position == null || navMeshAgent.destination == position || !navMeshAgent.enabled)
                return;

            if (!OnNavMesh.IsReachable(transform, position))
                return;

            navMeshAgent.destination = position;
            navMeshAgent.isStopped = false;
            navMeshAgent.stoppingDistance = navMeshAgent.isOnOffMeshLink ? 0.05f : 0.5f;
        }
    }

    public void StopNavigation()
    {
        if (!navMeshAgent.enabled)
            return;

        navMeshAgent.isStopped = true;
        navMeshAgent.ResetPath();
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
            UseAbility(9);
        }
        //Abilities
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            UseAbility(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            UseAbility(2);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            UseAbility(3);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            UseAbility(4);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            UseAbility(5);
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            UseAbility(6);
        }
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            UseAbility(7);
        }
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            UseAbility(8);
        }
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            UseAbility(9);
        }

        //Camera
        if (Input.GetKeyDown(KeyCode.C))
        {
            CamFollow();
        }
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            Interact();
        }
        //if (Input.GetKeyDown(KeyCode.U))
        //{
        //    if (climbing)
        //    {
        //        climbing = false;
        //    }
        //    else
        //    {
        //        climbing = true;
        //    }
        //}
    }
    public bool PointerOverUI()
    {
        return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }
}

