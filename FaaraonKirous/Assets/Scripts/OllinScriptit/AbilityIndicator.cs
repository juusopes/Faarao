using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class AbilityIndicator : MonoBehaviour
{
    public LineRenderer line;
    public GameObject player;
    public GameObject target;
    private LevelController levelControl;
    public GameObject indicatorArea;
    public GameObject circle;
    //[HideInInspector]
    public string targetTag;

    public Vector3 playerPos;
    public Vector3 endPoint;
    private float range;
    private bool abilityClicked;
    private int previousAbilityNum;

    private bool calculateNeeded;
    //AbilityTargets

    //Line Of Sight
    private bool hasLineOfSight;
    private Vector3 lineOfSightPoint;
    private bool lineOfSightPointBool;
    private bool allOk;


    private bool tempLineOfSight = false;
    private bool tempInRange = false;
    Vector3 mouseHitPos;
    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        MoveInd();
        TargetTags();
        ChangeAll();
        AbilityCastingConditions();
        LineColorChanger();

        //Position
        circle.transform.position = player.transform.position;
    }
    private void Initialize()
    {
        line = transform.GetChild(0).gameObject.GetComponent<LineRenderer>();
        line.transform.parent = null;
        levelControl = GameObject.FindGameObjectWithTag("LevelController").GetComponent<LevelController>();
        previousAbilityNum = 0;
    }
    private void MoveInd()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit = new RaycastHit();
        //RaycastHit hit = RayCaster.ScreenPoint(Input.mousePosition, RayCaster.attackLayerMask);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, RayCaster.attackLayerMask))
        {
            if (hit.collider.tag == targetTag)
            {
                target = hit.collider.gameObject;
                levelControl.targetObject = hit.collider.gameObject;
                transform.position = target.transform.position;
            }
            else if (hit.collider.tag != "Indicator")
            {
                Vector3 targetV3 = hit.point;
                targetV3.y += 0.3f;
                transform.position = targetV3;
                target = null;
                levelControl.targetObject = null;
            }
        }
        LineCalculator();
    }
    private void LineCalculator()
    {
        if (target != null)
        {
            //Line Start
            line.SetPosition(0, player.transform.position);

            //Vector Top point
            Vector3 topPoint = player.transform.position + ((target.transform.position - player.transform.position) / 2);
            topPoint.y = 10;
            line.SetPosition(line.positionCount / 2, topPoint);

            //Line End
            line.SetPosition(line.positionCount - 1, target.transform.position);
        }
        else
        {
            //Line Start
            line.SetPosition(0, player.transform.position);

            //Shoot Ray To see if in line of sight
            //DoubleClick Check

            PointCalculator();
            //Vector Top point
            //Vector3 topPoint = player.transform.position + ((transform.position - player.transform.position) / 2);
            //topPoint.y = 10;
            //line.SetPosition(line.positionCount / 2, topPoint);

            //Line End
            line.SetPosition(line.positionCount - 1, transform.position);
        }
    }
    private void TargetTags()
    {
        if (player.GetComponent<PlayerController>().abilityNum == 7 || player.GetComponent<PlayerController>().abilityNum == 9)
        {
            targetTag = "Enemy";
            calculateNeeded = false;
        }
        else if (player.GetComponent<PlayerController>().abilityNum == 10)
        {
            targetTag = "Player";
            calculateNeeded = false;
        }
        else if (player.GetComponent<PlayerController>().abilityNum == 8)
        {
            targetTag = "TargetableObject";
            calculateNeeded = false;
        }
        else
        {
            calculateNeeded = true;
        }
    }

    private void ChangeAll()
    {
        if (player.GetComponent<PlayerController>().abilityNum > 0 && previousAbilityNum != player.GetComponent<PlayerController>().abilityNum)
        {
            SwitchIndicator(player.GetComponent<PlayerController>().abilityNum);
            SetCircleRange(player.GetComponent<PlayerController>().abilityNum);
            previousAbilityNum = player.GetComponent<PlayerController>().abilityNum;
        }
        if (player.GetComponent<PlayerController>().abilityNum == 0)
        {
            previousAbilityNum = 0;
        }
        //if (player.GetComponent<PlayerController>().abilityNum == 2)
        //{
        //    SwitchIndicator(1);
        //    SetCircleRange(1);
        //}
        //if (player.GetComponent<PlayerController>().abilityNum == 3)
        //{
        //    SwitchIndicator(2);
        //    SetCircleRange(2);
        //}
        //if (player.GetComponent<PlayerController>().abilityNum == 4)
        //{
        //    SwitchIndicator(3);
        //    SetCircleRange(3);
        //}
        //if (player.GetComponent<PlayerController>().abilityNum == 5)
        //{
        //    SwitchIndicator(7);
        //    SetCircleRange(7);
        //}
        //if (player.GetComponent<PlayerController>().abilityNum == 8)
        //{
        //    SwitchIndicator(8);
        //    SetCircleRange(8);
        //}
        //if (player.GetComponent<PlayerController>().abilityNum == 9)
        //{
        //    SwitchIndicator(9);
        //    SetCircleRange(9);
        //}
        //if (player.GetComponent<PlayerController>().abilityNum == 10)
        //{
        //    SwitchIndicator(10);
        //    SetCircleRange(10);
        //}
    }
    private void SetCircleRange(int num)
    {
        //Scale
        Vector3 scaleVector = circle.transform.localScale;
        if (player.GetComponent<PlayerController>().playerOne)
        {
            range = player.GetComponent<PharaohAbilities>().rangeList[num];
            scaleVector.x = range;
            scaleVector.z = range;
        }
        else
        {
            range = player.GetComponent<PriestAbilities>().rangeList[num];
            scaleVector.x = range;
            scaleVector.z = range;
        }
        circle.transform.localScale = scaleVector;
    }
    private void SwitchIndicator(int num)
    {
        if (player.GetComponent<PlayerController>().playerOne)
        {
            indicatorArea.transform.localScale = player.GetComponent<PharaohAbilities>().indicatorList[num].transform.localScale;
            //Switch Background
            indicatorArea.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite
                = player.GetComponent<PharaohAbilities>().indicatorList[num].transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite;
            //Switch Icon
            indicatorArea.transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>().sprite
                = player.GetComponent<PharaohAbilities>().indicatorList[num].transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>().sprite;
            //Switch Aim
            indicatorArea.transform.GetChild(2).gameObject.GetComponent<SpriteRenderer>().sprite
                = player.GetComponent<PharaohAbilities>().indicatorList[num].transform.GetChild(2).gameObject.GetComponent<SpriteRenderer>().sprite;
            indicatorArea.transform.GetChild(2).gameObject.transform.rotation = player.GetComponent<PharaohAbilities>().indicatorList[num].transform.GetChild(2).gameObject.transform.rotation;
            indicatorArea.transform.GetChild(2).gameObject.transform.localScale = player.GetComponent<PharaohAbilities>().indicatorList[num].transform.GetChild(2).gameObject.transform.localScale;

        }
        else
        {
            indicatorArea.transform.localScale = player.GetComponent<PriestAbilities>().indicatorList[num].transform.localScale;
            //Switch Background
            indicatorArea.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite
                = player.GetComponent<PriestAbilities>().indicatorList[num].transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite;
            //Switch Icon
            indicatorArea.transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>().sprite
                = player.GetComponent<PriestAbilities>().indicatorList[num].transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>().sprite;
            //Switch Aim
            indicatorArea.transform.GetChild(2).gameObject.GetComponent<SpriteRenderer>().sprite
                = player.GetComponent<PriestAbilities>().indicatorList[num].transform.GetChild(2).gameObject.GetComponent<SpriteRenderer>().sprite;
            indicatorArea.transform.GetChild(2).gameObject.transform.rotation = player.GetComponent<PriestAbilities>().indicatorList[num].transform.GetChild(2).gameObject.transform.rotation;
            indicatorArea.transform.GetChild(2).gameObject.transform.localScale = player.GetComponent<PriestAbilities>().indicatorList[num].transform.GetChild(2).gameObject.transform.localScale;
        }
    }
    private void AbilityCastingConditions()
    {
        if (calculateNeeded)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit = new RaycastHit();
            //RaycastHit hit = RayCaster.ScreenPoint(Input.mousePosition, RayCaster.attackLayerMask);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, RayCaster.attackLayerMask))
            {
                //vectorLength = vectorLength / 6.6f;
                if (Input.GetKeyDown(KeyCode.Mouse1))
                {
                    playerPos = player.GetComponent<PlayerController>().GetPosition();
                    Vector3 hitPos = hit.transform.position;
                    lineOfSightPoint = hit.transform.position;
                    lineOfSightPoint.y = hit.transform.position.y + 0.3f;
                    lineOfSightPointBool = true;
                    //hitPos.y = playerPos.y;
                    float distance = (Vector3.Distance(playerPos, hitPos)) / 4;
                    //LineOfSightCheck
                    if (Physics.Raycast(player.transform.position, lineOfSightPoint - player.transform.position, out hit, Vector3.Distance(player.transform.position, lineOfSightPoint) - 0.3f, RayCaster.attackLayerMask) && hit.collider.gameObject.tag != targetTag)
                    {
                        hasLineOfSight = false;
                        Debug.Log(hit.collider.gameObject);
                    }
                    else
                    {
                        hasLineOfSight = true;
                        lineOfSightPointBool = false;
                    }
                    //DistanceCheck
                    if (!hasLineOfSight)
                    {
                        player.GetComponent<PlayerController>().GiveDestination(hitPos);
                        player.GetComponent<PlayerController>().searchingForSight = true;
                        player.GetComponent<PlayerController>().inRange = false;
                        Debug.Log("CASE 1");
                    }
                    else if (distance > range + range * 0.1f)
                    {
                        endPoint = Vector3.MoveTowards(playerPos, hitPos, ((distance - range) * 4));
                        player.GetComponent<PlayerController>().GiveDestination(endPoint);
                        player.GetComponent<PlayerController>().inRange = false;
                        player.GetComponent<PlayerController>().searchingForSight = false;
                        Debug.Log("CASE 2: " + distance + ", " + range);
                    }
                    else
                    {
                        player.GetComponent<PlayerController>().inRange = true;
                        player.GetComponent<PlayerController>().searchingForSight = false;
                        Debug.Log("CASE 3");
                    }
                    player.GetComponent<PlayerController>().abilityClicked = true;

                   // Debug.Log("In Range: " + player.GetComponent<PlayerController>().inRange + ", Sight: " + player.GetComponent<PlayerController>().searchingForSight);
                }
            }
        }
        //if (player.GetComponent<PlayerController>().abilityClicked)
        //{
        //    if (endPoint.x == player.GetComponent<PlayerController>().GetPosition().x && endPoint.z == player.GetComponent<PlayerController>().GetPosition().z)
        //    {
        //        player.GetComponent<PlayerController>().inRange = true;
        //        //player.GetComponent<PlayerController>().abilityClicked = false;
        //        player.GetComponent<PlayerController>().searchingForSight = false;
        //    }
        //}
    }

    public void PointCalculator()
    {
        RaycastHit hit = new RaycastHit();
        if (lineOfSightPointBool)
        {
            if (Physics.Raycast(player.transform.position, lineOfSightPoint - player.transform.position, out hit, Vector3.Distance(player.transform.position, lineOfSightPoint) - 0.3f, RayCaster.attackLayerMask))
            {
                if (hit.collider.gameObject.tag != targetTag)
                {
                    hasLineOfSight = false;
                    player.GetComponent<PlayerController>().searchingForSight = true;
                }
            }
            else
            {
                hasLineOfSight = true;
                lineOfSightPointBool = false;
                player.GetComponent<PlayerController>().searchingForSight = false;
            }
        }
        else
        {
            playerPos = player.GetComponent<PlayerController>().GetPosition();
            // lineOfSightPoint.y = playerPos.y;
            //lineOfSightPoint.y += 0.3f;
            float distance = (Vector3.Distance(playerPos, lineOfSightPoint)) / 4;
            if (distance <= range + range * 0.1)
            {
                //endPoint = Vector3.MoveTowards(playerPos, hitPos, ((distance - range) * 4));
                //player.GetComponent<PlayerController>().GiveDestination(endPoint);
                //Debug.Log("CASE2");
                player.GetComponent<PlayerController>().inRange = true;
                if (player.GetComponent<PlayerController>().abilityClicked)
                {
                    player.GetComponent<PlayerController>().Stay();
                }
            }
        }
    }
    private void LineColorChanger()
    {
        //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //RaycastHit hit = new RaycastHit();
        //if (Physics.Raycast(ray, out hit, Mathf.Infinity, RayCaster.attackLayerMask))
        //{
        //    mouseHitPos = hit.transform.position;
        //}
        ////Debug.Log(player);
        //playerPos = player.GetComponent<PlayerController>().GetPosition();
        //playerPos.y = player.GetComponent<PlayerController>().GetPosition().y + 0.2f;
        //if (Physics.Raycast(playerPos, mouseHitPos, out hit, Vector3.Distance(player.transform.position, mouseHitPos) - 0.3f, RayCaster.attackLayerMask))
        //{
        //    tempLineOfSight = false;
        //    //Debug.Log(hit.collider.gameObject);
        //}
        //else
        //{
        //    tempLineOfSight = true;
        //    //Debug.Log("LineOfSight: true");
        //}
        //playerPos = player.GetComponent<PlayerController>().GetPosition();
        //float distance = (Vector3.Distance(playerPos, mouseHitPos)) / 4;
        //if (distance <= range)
        //{
        //    tempInRange = true;
        //    Debug.Log("Range: true");
        //}
        //else
        //{
        //    tempInRange = false;
        //    //Debug.Log(distance + ", " + range);
        //}
        //if (!tempLineOfSight || !tempInRange)
        //{
        //    line.startColor = Color.red;
        //    line.endColor = Color.red;
        //}
        //else
        //{
        //    line.startColor = Color.green;
        //    line.endColor = Color.green;
        //}
    }
}
