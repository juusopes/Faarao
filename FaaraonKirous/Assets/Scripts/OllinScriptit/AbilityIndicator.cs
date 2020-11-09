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
    //AbilityTargets


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
        DistanceCalculator();

        //Position
        circle.transform.position = player.transform.position;
    }
    private void Initialize()
    {
        line = transform.GetChild(0).gameObject.GetComponent<LineRenderer>();
        line.transform.parent = null;
        levelControl = GameObject.FindGameObjectWithTag("LevelController").GetComponent<LevelController>();
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

            //Vector Top point
            Vector3 topPoint = player.transform.position + ((transform.position - player.transform.position) / 2);
            topPoint.y = 10;
            line.SetPosition(line.positionCount / 2, topPoint);

            //Line End
            line.SetPosition(line.positionCount - 1, transform.position);
        }
    }
    private void TargetTags()
    {
        if (player.GetComponent<PlayerController>().abilityNum == 7 || player.GetComponent<PlayerController>().abilityNum == 9)
        {
            targetTag = "Enemy";
        }
    }

    private void ChangeAll()
    {
        if (player.GetComponent<PlayerController>().abilityNum == 1)
        {
            SwitchIndicator(0);
            SetCircleRange(0);
        }
        if (player.GetComponent<PlayerController>().abilityNum == 2)
        {
            SwitchIndicator(1);
            SetCircleRange(1);
        }
        if (player.GetComponent<PlayerController>().abilityNum == 3)
        {
            SwitchIndicator(2);
            SetCircleRange(2);
        }
        if (player.GetComponent<PlayerController>().abilityNum == 4)
        {
            SwitchIndicator(3);
            SetCircleRange(3);
        }
        if (player.GetComponent<PlayerController>().abilityNum == 5)
        {
            SwitchIndicator(4);
            SetCircleRange(4);
        }
        if (player.GetComponent<PlayerController>().abilityNum == 9)
        {
            SwitchIndicator(5);
            SetCircleRange(5);
        }
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

        } else
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
    private void DistanceCalculator()
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
                hitPos.y = playerPos.y;
                float distance = (Vector3.Distance(playerPos, hitPos)) / 4;
                Debug.Log(distance);
                if (distance > range)
                {
                    endPoint = Vector3.MoveTowards(playerPos, hitPos, ((distance-range)*4));
                    player.GetComponent<PlayerController>().GiveDestination(endPoint);
                }
                abilityClicked = true;
            }
        }
        if (abilityClicked)
        {
            if (endPoint.x == player.GetComponent<PlayerController>().GetPosition().x && endPoint.z == player.GetComponent<PlayerController>().GetPosition().z)
            {
                player.GetComponent<PlayerController>().inRange = true;
                abilityClicked = false;
            }
        }
    }
}
