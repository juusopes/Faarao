﻿using UnityEngine;
using UnityEngine.Rendering;

public class PriestAbilities : MonoBehaviour
{
    private PlayerController pharaoh;
    //Telekinesis
    private GameObject target;
    private LevelController levelControl;
    private bool telekinesisActive;
    public bool useTeleknesis;
    private float telekinesisTimer;
    private Vector3 telekinesisHeight;
    //WarpSpell
    public bool warpSpellActive;
    private Vector3 warpPosition;    

    private Vector3 playerSavePos;
    private Vector3 targetSavePos;
    public GameObject[] indicatorList;
    public float[] rangeList;
    public bool[] lineActive;

    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {   
        Telekinesis();
        WarpPosition();
    }

    private void Initialize()
    {
        levelControl = GameObject.FindGameObjectWithTag("LevelController").GetComponent<LevelController>();
        telekinesisActive = false;
        telekinesisTimer = 5;
    }

    public void Telekinesis()
    {
        if (GetComponent<PlayerController>().IsCurrentPlayer)
        {
            //TempSetActive
            if (GetComponent<PlayerController>().abilityActive && GetComponent<PlayerController>().abilityNum == 1)
            {
                telekinesisActive = true;
                if (GetComponent<PlayerController>().visibleInd != null)
                {
                    GetComponent<PlayerController>().visibleInd.GetComponent<AbilityIndicator>().targetTag = "TargetableObject";
                }
            }

            //TelekinesisSpell
            if (levelControl.targetObject != null)
            {
                target = levelControl.targetObject;
            } else if (!useTeleknesis)
            {
                target = null;
            }
            if (target != null)
            {
                if (telekinesisActive && GetComponent<PlayerController>().abilityNum == 6)
                {
                    if (Input.GetKeyDown(KeyCode.Mouse1))
                    {
                        if (target.tag == "TargetableObject")
                        {
                            telekinesisTimer = 0;
                            useTeleknesis = true;

                            target.GetComponent<Rigidbody>().isKinematic = true;
                            GetComponent<PlayerController>().abilityActive = false;
                            GetComponent<PlayerController>().abilityNum = 0;
                            GetComponent<PlayerController>().visibleInd.GetComponent<AbilityIndicator>().targetTag = null;
                        }
                    }
                }
            }
        }
        if (telekinesisTimer <= 4)
        {
            telekinesisTimer += Time.deltaTime;
        }
        if (telekinesisTimer < 1)
        {
            float yAxisValue = 0.01f;
            target.transform.Translate(new Vector3(0, yAxisValue, 0));
        }
        else if (telekinesisTimer > 1 && telekinesisTimer < 2)
        {
            telekinesisHeight = target.transform.position;
            playerSavePos = transform.position;
            targetSavePos = target.transform.position;
            telekinesisTimer = 2;
        }
        else if (telekinesisTimer >= 2 && telekinesisTimer < 4)
        {
            target.transform.Translate((playerSavePos - targetSavePos).normalized * Time.deltaTime * 5);
            target.transform.position = new Vector3(target.transform.position.x, telekinesisHeight.y, target.transform.position.z);
        }
        if (telekinesisTimer >= 4 && useTeleknesis)
        {
            target.GetComponent<Rigidbody>().isKinematic = false;
            telekinesisActive = false;
            useTeleknesis = false;
            target = null;
        }
    }

    public void WarpPosition()
    {
        if (GetComponent<PlayerController>().IsCurrentPlayer)
        {
            //TempSetActive
            if (GetComponent<PlayerController>().abilityActive && GetComponent<PlayerController>().abilityNum == 1)
            {
                warpSpellActive = true;
            }
            //WarpSpell
            if (levelControl.targetObject != null)
            {
                target = levelControl.targetObject;
            }
            else if (!useTeleknesis)
            {
                target = null;
            }
            if (warpSpellActive && GetComponent<PlayerController>().abilityNum == 1)
            {
                if (Input.GetKeyDown(KeyCode.Mouse1) && !PointerOverUI())
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit = new RaycastHit();
                    Debug.Log("WARP!");
                    //DoubleClick Check
                    if (Physics.Raycast(ray, out hit, Mathf.Infinity, RayCaster.attackLayerMask))
                    {
                        Debug.Log("MORE WARP!");
                        warpPosition = hit.point;
                    }
                }
                if (GetComponent<PlayerController>().inRange && GetComponent<PlayerController>().abilityClicked && !GetComponent<PlayerController>().searchingForSight)
                {
                    this.gameObject.GetComponent<PlayerController>().navMeshAgent.Warp(warpPosition);
                    warpPosition = transform.position;
                    //GetComponent<PlayerController>().abilityActive = false;
                    //GetComponent<PlayerController>().abilityNum = 0;
                }
            }
        }
    }

    private void TelekinesisActivate()
    {
        if (telekinesisActive)
        {
            telekinesisActive = false;
        }
        else
        {
            telekinesisActive = false;
        }
    }

    public bool PointerOverUI()
    {
        return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }
}
