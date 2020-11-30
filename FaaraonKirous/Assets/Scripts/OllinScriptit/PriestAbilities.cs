using UnityEngine;
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
    public bool warped;
    //private Vector3 playerSavePos;
    //private Vector3 targetSavePos;
    public GameObject[] indicatorList;
    public float[] rangeList;
    public int[] abilityLimitList;
    public float[] abilityCDList;
    public bool[] lineActive;

    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {   
        //Telekinesis();
        WarpPosition();
    }

    private void Initialize()
    {
        levelControl = GameObject.FindGameObjectWithTag("LevelController").GetComponent<LevelController>();
        telekinesisActive = false;
        telekinesisTimer = 5;
    }

    //public void Telekinesis()
    //{
    //    if (GetComponent<PlayerController>().IsCurrentPlayer)
    //    {
    //        //TempSetActive
    //        if (GetComponent<PlayerController>().abilityActive && GetComponent<PlayerController>().abilityNum == 1)
    //        {
    //            telekinesisActive = true;
    //            if (GetComponent<PlayerController>().visibleInd != null)
    //            {
    //                GetComponent<PlayerController>().visibleInd.GetComponent<AbilityIndicator>().targetTag = "TargetableObject";
    //            }
    //        }

    //        //TelekinesisSpell
    //        if (levelControl.targetObject != null)
    //        {
    //            target = levelControl.targetObject;
    //        } else if (!useTeleknesis)
    //        {
    //            target = null;
    //        }
    //        if (target != null)
    //        {
    //            if (telekinesisActive && GetComponent<PlayerController>().abilityNum == 6)
    //            {
    //                if (Input.GetKeyDown(KeyCode.Mouse1))
    //                {
    //                    if (target.tag == "TargetableObject")
    //                    {
    //                        telekinesisTimer = 0;
    //                        useTeleknesis = true;

    //                        target.GetComponent<Rigidbody>().isKinematic = true;
    //                        GetComponent<PlayerController>().abilityActive = false;
    //                        GetComponent<PlayerController>().abilityNum = 0;
    //                        GetComponent<PlayerController>().visibleInd.GetComponent<AbilityIndicator>().targetTag = null;
    //                    }
    //                }
    //            }
    //        }
    //    }
    //    if (telekinesisTimer <= 4)
    //    {
    //        telekinesisTimer += Time.deltaTime;
    //    }
    //    if (telekinesisTimer < 1)
    //    {
    //        float yAxisValue = 0.01f;
    //        target.transform.Translate(new Vector3(0, yAxisValue, 0));
    //    }
    //    else if (telekinesisTimer > 1 && telekinesisTimer < 2)
    //    {
    //        telekinesisHeight = target.transform.position;
    //        playerSavePos = transform.position;
    //        targetSavePos = target.transform.position;
    //        telekinesisTimer = 2;
    //    }
    //    else if (telekinesisTimer >= 2 && telekinesisTimer < 4)
    //    {
    //        target.transform.Translate((playerSavePos - targetSavePos).normalized * Time.deltaTime * 5);
    //        target.transform.position = new Vector3(target.transform.position.x, telekinesisHeight.y, target.transform.position.z);
    //    }
    //    if (telekinesisTimer >= 4 && useTeleknesis)
    //    {
    //        target.GetComponent<Rigidbody>().isKinematic = false;
    //        telekinesisActive = false;
    //        useTeleknesis = false;
    //        target = null;
    //    }
    //}

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
                    if (Physics.Raycast(ray, out hit, Mathf.Infinity, RayCaster.attackLayerMask))
                    {
                        warpPosition = hit.point;
                    }
                }
                if (GetComponent<PlayerController>().inRange
                    && GetComponent<PlayerController>().abilityClicked
                    && !GetComponent<PlayerController>().searchingForSight
                    && GetComponent<PlayerController>().abilityLimits[GetComponent<PlayerController>().abilityNum] > 0
                    && GetComponent<PlayerController>().abilityCooldowns[GetComponent<PlayerController>().abilityNum] == 0
                    && (warpPosition.x > this.gameObject.transform.position.x + 0.5f || warpPosition.x < this.gameObject.transform.position.x - 0.5f)
                     && (warpPosition.y > this.gameObject.transform.position.y + 0.5f || warpPosition.y < this.gameObject.transform.position.y - 0.5f)
                      && (warpPosition.z > this.gameObject.transform.position.z + 0.5f || warpPosition.z < this.gameObject.transform.position.z - 0.5f))
                {
                    if (warpPosition.y < this.gameObject.transform.position.y + 5f)
                    {
                        if (NetworkManager._instance.IsHost)
                        {
                            GetComponent<PlayerController>().navMeshAgent.Warp(warpPosition);
                        }
                        else
                        {
                            if (NetworkManager._instance.ShouldSendToServer)
                            {
                                ClientSend.Warp(GetComponent<PlayerObjectManager>().Type, warpPosition);
                            }
                        }
                        warpSpellActive = false;
                        warped = true;
                        //GetComponent<PlayerController>().abilityLimits[GetComponent<PlayerController>().abilityNum]--;
                    }
                    //GetComponent<PlayerController>().abilityActive = false;
                    //GetComponent<PlayerController>().abilityNum = 0;
                }
            }
        }
    }

    public bool PointerOverUI()
    {
        return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }
}
