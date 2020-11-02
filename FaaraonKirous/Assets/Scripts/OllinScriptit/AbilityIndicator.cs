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
    //[HideInInspector]
    public string targetTag;

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
        IndicatorChange();
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
        if (Physics.Raycast(ray, out hit))
        {
            Debug.Log(hit.collider.tag);
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
        if (player.GetComponent<PlayerController>().abilityNum == 7)
        {
            targetTag = "Enemy";
        }
    }

    private void IndicatorChange()
    {
        if (player.GetComponent<PlayerController>().abilityNum == 1)
        {
            SwitchIndicator(0);
        }
        if (player.GetComponent<PlayerController>().abilityNum == 2)
        {
            SwitchIndicator(1);
        }
        if (player.GetComponent<PlayerController>().abilityNum == 3)
        {
            SwitchIndicator(2);
        }
        if (player.GetComponent<PlayerController>().abilityNum == 4)
        {
            SwitchIndicator(3);
        }
        if (player.GetComponent<PlayerController>().abilityNum == 5)
        {
            SwitchIndicator(4);
        }
        if (player.GetComponent<PlayerController>().abilityNum == 9)
        {
            SwitchIndicator(5);
        }
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
}
