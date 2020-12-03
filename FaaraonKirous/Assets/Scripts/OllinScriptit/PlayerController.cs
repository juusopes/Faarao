using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    // Managers
    private PlayerObjectManager PlayerObjectManager { get; set; }

    //Which Player
    public bool playerOne;

    //Moving
    public float movementSpeed;
    public float doubleClickTimer;
    public bool IsCurrentPlayer { get; set; } = false;
    public bool IsActivePlayer { get; set; } = false;
    public NavMeshAgent navMeshAgent;
    private Vector3 targetV3;
    private Vector3 position;

    public bool IsRunning { get; set; } = false;

    //PlayerSynergy
    [HideInInspector]
    public GameObject anotherCharacter;
    private bool lineOfSight;

    public bool IsCrouching { get; set; } = false;

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
    public bool searchingForSight;
    public bool abilityClicked;
    public Vector3 abilityHitPos;
    //[HideInInspector]
    public bool[] abilityAllowed;

    //Invisibility
    public bool isInvisible;
    private Material originalMaterial;
    public Material invisibilityMaterial;

    //Camera
    private GameObject camControl;

    //Interactive
    public GameObject interactObject;
    private bool useInteract;

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

    //Respawn
    private bool useRespawn;
    
    //Menu
    public LevelController lC;
    public InGameMenu menu;

    public bool startDead;

    //Animations
    public Animator anim;
    private Vector3 movingCheck;

    //Ability Limits
    public int abilityLimitUsed;
    public int[] abilityLimits;
    public float[] abilityCooldowns;

    public UnitInteractions unitInteractions;

    private void Awake()
    {
        Initialize();
    }

    private void Start()
    {
        if (startDead)
        {
            death.Die(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        CooldownCheck();
        Die();
        if (!IsDead)
        {
            Moving();
            LineOfSight();
            if (!menu.menuActive && IsCurrentPlayer && !DontDestroyCanvas.Instance.IsOpen())
            {
                //print("MENU EI OO AKTIIVINE JA CURRENTPLAYER");
                KeyControls();
            } else
            {
                abilityActive = false;
            }
            Invisibility();  // TODO: Does not work in multiplayer
            if (NetworkManager._instance.IsHost)
            {
                TestOffLink();
            }
            Attack();
            Respawn();
            Interact();
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
        if (!IsCurrentPlayer)
        {
            abilityActive = false;
        }
        Animations();
    }
    private void Initialize()
    {
        PlayerObjectManager = GetComponent<PlayerObjectManager>();
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

        //AbilityLimits & Cooldowns
        if (playerOne)
        {
            abilityCooldowns = new float[GetComponent<PharaohAbilities>().abilityCDList.Length];
            //for (int x = 0; x < abilityCooldowns.Length-1; x++)
            //{
            //    abilityCooldowns[x] = 0;
            //}
        } else
        {
            abilityCooldowns = new float[GetComponent<PriestAbilities>().abilityCDList.Length];
            //for (int x = 0; x < abilityCooldowns.Length - 1; x++)
            //{
            //    abilityCooldowns[x] = 0;
            //}
        }
		
        ResetAbilityLimits();
        originalMaterial = transform.GetChild(0).transform.GetChild(0).GetComponent<Renderer>().material;
    }

    public void Moving()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit = new RaycastHit();
        //DoubleClick Check
        if (IsCurrentPlayer)
        {
            if (Input.GetKeyDown(KeyCode.Mouse1) && !PointerOverUI())
            {
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, RayCaster.attackLayerMask))
                {
                    targetV3 = hit.point;
                    SetDestination(targetV3);
                }
                if (IsRunning)
                {
                    SetRunning(false);
                }
                if (doubleClickTimer < 0.5f && !IsRunning)
                {
                    SetRunning(true);
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
                if (position == transform.position && IsRunning)
                {
                    SetRunning(false);
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
        if (NetworkManager._instance.IsHost)
        {
            SetDestination(transform.position);
            targetV3 = transform.position;
        }
        else if (NetworkManager._instance.ShouldSendToServer)
        {
            ClientSend.Stay(PlayerObjectManager.Type);
        }
    }
    
    public void GiveDestination(Vector3 v3)
    {
        targetV3 = v3;
        SetDestination(targetV3);
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
        if(!IsDead)
        {
            unitInteractions.StanceCheckUI();

            if (IsCurrentPlayer && !IsDead)
            {
                if (!IsCrouching)
                {
                    if (IsRunning)
                    {
                        SetRunning(false);
                    }

                    SetCrouching(true);
                }
                else
                {
                    SetCrouching(false);
                }
            }
        }
    }

    private void SetRunning(bool state)
    {
        IsRunning = state;

        if (NetworkManager._instance.ShouldSendToClient)
        {
            ServerSend.Running(PlayerObjectManager.Type, state);
        }
        else if (NetworkManager._instance.ShouldSendToServer)
        {
            ClientSend.Running(PlayerObjectManager.Type, state);
        }
    }

    private void SetCrouching(bool state)
    {
        IsCrouching = state;

        if (NetworkManager._instance.ShouldSendToClient)
        {
            ServerSend.Crouching(PlayerObjectManager.Type, state);
        }
        else if (NetworkManager._instance.ShouldSendToServer)
        {
            ClientSend.Crouching(PlayerObjectManager.Type, state);
        }
    }

    private void Invisibility()
    {
        if (isInvisible)
        {
            this.gameObject.tag = "PlayerInvisible";
            transform.GetChild(0).transform.GetChild(0).GetComponent<Renderer>().material = invisibilityMaterial;
        }
        else
        {
            this.gameObject.tag = "Player";
            transform.GetChild(0).transform.GetChild(0).GetComponent<Renderer>().material = originalMaterial;
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
        if (IsCurrentPlayer)
        {
            if (abilityNum == 8)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit = new RaycastHit();
                //DoubleClick Check

                if (!useInteract)
                {
                    target = null;
                }

                if (Input.GetKeyDown(KeyCode.Mouse0) && IsCurrentPlayer && !PointerOverUI())
                {
                    if (lC.targetObject != null)
                    {
                        target = lC.targetObject;
                        if (target.tag == "TargetableObject")
                        {
                            if (Physics.Raycast(ray, out hit, Mathf.Infinity, RayCaster.attackLayerMask))
                            {
                                targetV3 = hit.point;
                                SetDestination(targetV3);
                            }

                            useInteract = true;
                            abilityActive = false;
                        }
                    }
                }
                if (interactObject != null)
                {
                    if (interactObject == target)
                    {
                        if (NetworkManager._instance.IsHost)
                        {
                            interactObject.gameObject.GetComponent<ActivatorScript>().Activate();
                        }
                        else
                        {
                            if (NetworkManager._instance.ShouldSendToServer)
                            {
                                ClientSend.ActivateObject(interactObject.GetComponent<ActivatableObjectManager>().Id);
                            }
                        }
                        interactObject = null;
                        target = null;
                        useInteract = false;
                        abilityNum = 0;
                        Stay();
                    }
                }
            }
        }
    }
    public void Attack()
    {
        if (IsCurrentPlayer)
        {
            if (abilityNum == 9)
            {
                //if (IsCurrentPlayer)
                //{
                //    GetComponent<PlayerController>().visibleInd.GetComponent<AbilityIndicator>().targetTag = "Enemy";
                //}
                if (!useAttack)
                {
                    target = null;
                }
                if (Input.GetKeyDown(KeyCode.Mouse0) && IsCurrentPlayer)
                {
                    if (lC.targetObject != null)
                    {
                        target = lC.targetObject;
                        if (target.tag == "Enemy")
                        {
                            targetV3 = target.transform.position;
                            SetDestination(targetV3);
                            useAttack = true;
                            abilityActive = false;
                        }
                    }
                }
                if (targetEnemy != null)
                {
                    if (targetEnemy == target)
                    {
                        if (NetworkManager._instance.IsHost)
                        {
                            targetEnemy.GetComponent<DeathScript>().Die();
                            isInvisible = false;
                        }
                        else
                        {
                            if (NetworkManager._instance.ShouldSendToServer)
                            {
                                ClientSend.KillEnemy(targetEnemy.GetComponent<EnemyObjectManager>().Id);
                            }
                        }
                        anim.SetTrigger("Attack");
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

    public void Respawn()
    {
        if (IsCurrentPlayer)
        {
            if (abilityNum == 10)
            {
                //if (IsCurrentPlayer)
                //{
                //    GetComponent<PlayerController>().visibleInd.GetComponent<AbilityIndicator>().targetTag = "Player";
                //}
                if (!useRespawn)
                {
                    target = null;
                }
                if (Input.GetKeyDown(KeyCode.Mouse0) && IsCurrentPlayer)
                {
                    if (lC.targetObject != null)
                    {
                        target = lC.targetObject;
                        if (target.tag == "Player")
                        {
                            targetV3 = target.transform.position;
                            SetDestination(targetV3);
                            useRespawn = true;
                            abilityActive = false;
                        }
                    }
                }
                if (targetEnemy != null)
                {
                    if (targetEnemy == target)
                    {
                        if (NetworkManager._instance.IsHost)
                        {
                            targetEnemy.GetComponent<DeathScript>().Revive();
                            isInvisible = false;
                        }
                        else
                        {
                            if (NetworkManager._instance.ShouldSendToServer)
                            {
                                ClientSend.Revive(targetEnemy.GetComponent<PlayerObjectManager>().Id);
                            }
                        }
                        
                        targetEnemy = null;
                        target = null;
                        useRespawn = false;
                        abilityNum = 0;
                        Stay();
                    }
                }
            }
        }
    }
    public void UseAbility(int tempAbilityNum)
    {
        if (abilityAllowed[tempAbilityNum])
        {
            if (playerOne)
            {
                if ((abilityLimits[tempAbilityNum] > 0 && abilityCooldowns[tempAbilityNum] == 0) || (GetComponent<PharaohAbilities>().abilityLimitList[tempAbilityNum] == 0 && abilityCooldowns[tempAbilityNum] == 0))
                {
                    abilityClicked = false;
                    searchingForSight = true;
                    inRange = false;
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
            } else
            {
                if ((abilityLimits[tempAbilityNum] > 0 && abilityCooldowns[tempAbilityNum] == 0) || GetComponent<PriestAbilities>().abilityLimitList[tempAbilityNum] == 0)
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
            }
        }
    }
    public void ResetAbilityLimits()
    {
        if (playerOne)
        {
            abilityLimits = new int[GetComponent<PharaohAbilities>().abilityLimitList.Length];
            GetComponent<PharaohAbilities>().abilityLimitList.CopyTo(abilityLimits, 0);
        }
        else
        {
            abilityLimits = new int[GetComponent<PriestAbilities>().abilityLimitList.Length];
            GetComponent<PriestAbilities>().abilityLimitList.CopyTo(abilityLimits, 0);
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

    public void SetDestination(Vector3 position)
    {
        if (NetworkManager._instance.IsHost)
        {
            if (position == null || navMeshAgent.destination == position || !navMeshAgent.enabled)
                return;

            if (!OnNavMesh.IsCompletelyReachable(transform, position))
                return;

            navMeshAgent.destination = position;
            navMeshAgent.isStopped = false;
            navMeshAgent.stoppingDistance = navMeshAgent.isOnOffMeshLink ? 0.05f : 0.5f;
        }
        else
        {
            if (NetworkManager._instance.ShouldSendToServer)
            {
                ClientSend.SetDestinationRequest(PlayerObjectManager.Type, position);
            }
        }
    }

    public void StopNavigation()
    {
        if (!navMeshAgent.enabled)
            return;

        navMeshAgent.isStopped = true;
        navMeshAgent.ResetPath();
    }

    private void Animations()
    {
        if (IsCrouching)
        {
            anim.SetBool("IsCrouching", true);
        } else
        {
            anim.SetBool("IsCrouching", false);
        }
        if (IsRunning)
        {
            anim.SetBool("IsRunning", true);
        }
        else
        {
            anim.SetBool("IsRunning", false);
        }
        if (movingCheck == transform.position)
        {
            anim.SetBool("IsWalking", false);
        } else
        {
            anim.SetBool("IsWalking", true);
            movingCheck = transform.position;
        }
    }
    private void KeyControls()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Crouch();
        }
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            UseAbility(8);
        }
        //if (Input.GetKeyDown(KeyCode.I))
        //{
        //    InvisiblitySpell();
        //}
        if (Input.GetKeyDown(KeyCode.A))
        {
            UseAbility(9);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            UseAbility(10);
        }
        //Abilities
        if (Input.GetKeyDown(KeyCode.Q))
        {
            UseAbility(1);
        }

        ////////////////swapped!
        if (Input.GetKeyDown(KeyCode.E))
        {
            UseAbility(2);
        }
        ////////////////swapped!
        if (Input.GetKeyDown(KeyCode.W))
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

        //Camera
        if (Input.GetKeyDown(KeyCode.C))
        {
            CamFollow();
        }
    }
    public bool PointerOverUI()
    {
        if (UnityEngine.EventSystems.EventSystem.current == null)
            return false;
        //return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
        return UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject != null;
    }

    public void CooldownCheck()
    {
        if (abilityLimitUsed > 0)
        {
            abilityLimits[abilityLimitUsed]--;   
            if (playerOne) {
                abilityCooldowns[abilityLimitUsed] = GetComponent<PharaohAbilities>().abilityCDList[abilityLimitUsed];
            }
            else
            {
                abilityCooldowns[abilityLimitUsed] = GetComponent<PriestAbilities>().abilityCDList[abilityLimitUsed];
            }
            //            public bool[] abilityAllowed;
            //public float[] abilityCooldowns;
            abilityLimitUsed = 0;
        }
        for (int x = 1; x < abilityCooldowns.Length-1; x++) {
            if (abilityCooldowns[x] > 0)
            {
                abilityCooldowns[x] -= Time.deltaTime;
            } else
            {
                abilityCooldowns[x] = 0;
            }
        }
    }
    private void Die()
    {
        if (IsDead)
        {
            transform.GetChild(0).gameObject.SetActive(false);
            transform.GetChild(1).gameObject.SetActive(true);
        } else
        {
            transform.GetChild(0).gameObject.SetActive(true);
            transform.GetChild(1).gameObject.SetActive(false);
        }
    }
}

