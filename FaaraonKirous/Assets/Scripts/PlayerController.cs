using System.Collections;
using System.Collections.Generic;
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
    [HideInInspector]
    public bool isCrouching;

    //Invisibility
    public bool isInvisible;
    private float invisibilityTimer;

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
    }

    private void Initialize()
    {
        navMeshAgent = this.GetComponent<NavMeshAgent>();
        targetV3 = transform.position;
        invisibilityTimer = 10;

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
                if (doubleClickTimer >= 0.5f)
                {
                    doubleClickTimer = 0;
                }
                if (doubleClickTimer < 0.5f)
                {
                    isRunning = true;
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
            navMeshAgent.speed = movementSpeed * 1.5f;
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
            Debug.Log("RayHIT!");
            if (hit.collider.tag != "Player")
            {
                lineOfSight = false;
                Debug.Log("RayHIT!" + hit.collider);
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
                Material newMat = new Material(this.gameObject.GetComponent<Renderer>().material.shader);
                newMat.color = Color.blue;
                this.gameObject.GetComponent<Renderer>().material = newMat;
            }
            else
            {
                isCrouching = false;
                Material newMat = new Material(this.gameObject.GetComponent<Renderer>().material.shader);
                newMat.color = Color.green;
                this.gameObject.GetComponent<Renderer>().material = newMat;
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
        } else
        {
            Material newMat = new Material(this.gameObject.GetComponent<Renderer>().material.shader);
            newMat.color = Color.green;
            this.gameObject.GetComponent<Renderer>().material = newMat;
        }
    }

    private void KeyControls()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            Crouch();
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            InvisiblitySpell();
        }
    }
}
