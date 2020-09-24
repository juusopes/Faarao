using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
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
    public bool abilityIsActive;
    public GameObject indicator;
    private GameObject line;
    private GameObject visibleInd;

    //Invisibility
    public bool isInvisible;
    private float invisibilityTimer;

    //Camera
    private GameObject camControl;

    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        Moving();
        LineOfSight();
        KeyControls();
        Invisibility();
        SetIndicator();
    }

    private void Initialize()
    {
        navMeshAgent = this.GetComponent<NavMeshAgent>();
        targetV3 = transform.position;
        invisibilityTimer = 10;
        camControl = GameObject.FindGameObjectWithTag("MainCamera").transform.parent.gameObject;

        GameObject[] tempCharacters = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject tempCharacter in tempCharacters)
        {
            if (tempCharacter != this.gameObject)
            {
                anotherCharacter = tempCharacter;
            }
        }
    }
    public void Moving()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit = new RaycastHit();
        //DoubleClick Check
        if (isActiveCharacter)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (Physics.Raycast(ray, out hit))
                {
                    targetV3 = hit.point;
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
        if (isRunning)
        {
            navMeshAgent.speed = movementSpeed * 5f;
        }
        else if (isCrouching)
        {
            navMeshAgent.speed = movementSpeed * 0.2f;
        }
        else
        {
            navMeshAgent.speed = movementSpeed;
        }
        navMeshAgent.SetDestination(targetV3);
        if (position == transform.position)
        {
            isRunning = false;
        }
        else
        {
            position = transform.position;
        }
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
            if (!isCrouching)
            {
                isCrouching = true;
            }
            else
            {
                isCrouching = false;
            }
        }
    }
    private void InvisiblitySpell()
    {
        if (isActiveCharacter)
        {
            if (lineOfSight)
            {
                anotherCharacter.GetComponent<PlayerController>().isInvisible = true;
            }
        }
    }

    private void Invisibility()
    {
        if (isInvisible)
        {
            if (invisibilityTimer > 5 && invisibilityTimer < 6)
            {
                isInvisible = false;
                invisibilityTimer = 10;
            }

            if (invisibilityTimer > 6)
            {
                invisibilityTimer = 0;
            }
            if (invisibilityTimer <= 6)
            {
                invisibilityTimer += Time.deltaTime;
                Color tempColor = Color.blue;
                Material newMat = new Material(this.gameObject.GetComponent<Renderer>().material.shader);
                newMat.color = Color.black;
                this.gameObject.GetComponent<Renderer>().material = newMat;
            }
        }
        else
        {
            Material newMat = new Material(this.gameObject.GetComponent<Renderer>().material.shader);
            newMat.color = Color.green;
            this.gameObject.GetComponent<Renderer>().material = newMat;
        }
    }

    private void SetIndicator()
    {
        if (isActiveCharacter)
        {
            if (abilityIsActive)
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
            } else
            {
                camControl.GetComponent<CameraControl>().camFollow = true;
                camControl.transform.parent = this.gameObject.transform;
            }
        }
    }

    private void KeyControls()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Crouch();
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            InvisiblitySpell();
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            if (!abilityIsActive)
            {
                abilityIsActive = true;
            }
            else
            {
                abilityIsActive = false;
            }
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            CamFollow();
        }
    }
}

