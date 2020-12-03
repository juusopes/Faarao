using UnityEngine;

public class PharaohAbilities : MonoBehaviour
{
    private PlayerController priest;
    //Invisibility
    private GameObject target;
    private LevelController levelControl;
    private bool invisibilityActive;
    public bool useInvisibility;
    private float invisibilityTimer;
    public float timeInInvisibility;
    public GameObject[] indicatorList;
    public float[] rangeList;
    public int[] abilityLimitList;
    public float[] abilityCDList;
    public bool[] lineActive;
    private bool invisibilityClicked;
    private GameObject invisibilityTarget;

    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        Invisibility();
    }

    private void Initialize()
    {
        levelControl = GameObject.FindGameObjectWithTag("LevelController").GetComponent<LevelController>();
        invisibilityActive = false;
        invisibilityTimer = timeInInvisibility * 10;
    }

    public void Invisibility()
    {
        if (GetComponent<PlayerController>().IsCurrentPlayer)
        {
            //TempSetActive
            if (GetComponent<PlayerController>().abilityActive && GetComponent<PlayerController>().abilityNum == 1)
            {
                invisibilityActive = true;
                if (GetComponent<PlayerController>().visibleInd != null)
                {
                    GetComponent<PlayerController>().visibleInd.GetComponent<AbilityIndicator>().targetTag = "Player";
                }
            }

            //InvisibilitySpell
            if (levelControl.targetObject != null)
            {
                target = levelControl.targetObject;
            }
            else if (!useInvisibility)
            {
                target = null;
            }
            if (invisibilityActive && GetComponent<PlayerController>().abilityNum == 1)
            {
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    invisibilityClicked = true;
                    if (target != null)
                    {
                        if (target.tag == "Player")
                        {
                            invisibilityTarget = target;
                        } else
                        {
                            invisibilityTarget = null;
                        }
                    }
                    else
                    {
                        invisibilityTarget = null;
                    }
                }
                if (invisibilityTarget != null)
                {
                    if (GetComponent<PlayerController>().inRange
                    && GetComponent<PlayerController>().abilityClicked
                    && !GetComponent<PlayerController>().searchingForSight
                    && GetComponent<PlayerController>().abilityLimits[GetComponent<PlayerController>().abilityNum] > 0
                    && GetComponent<PlayerController>().abilityCooldowns[GetComponent<PlayerController>().abilityNum] == 0
                    && invisibilityTarget.tag == "Player"
                    && invisibilityClicked)
                    {
                        invisibilityTarget.GetComponent<PlayerController>().isInvisible = true;
                        invisibilityTimer = 0;
                        useInvisibility = true;
                        //GetComponent<PlayerController>().Stay();
                        //GetComponent<PlayerController>().abilityActive = false;
                        //GetComponent<PlayerController>().abilityNum = 0;
                        GetComponent<PlayerController>().visibleInd.GetComponent<AbilityIndicator>().targetTag = null;
                        invisibilityClicked = false;
                    }
                    //Debug.Log("iR: " + GetComponent<PlayerController>().inRange);
                    //Debug.Log("AC: " + GetComponent<PlayerController>().abilityClicked);
                    //Debug.Log("SfS: " + GetComponent<PlayerController>().searchingForSight);
                    //Debug.Log("AL: " + GetComponent<PlayerController>().abilityLimits[GetComponent<PlayerController>().abilityNum]);
                    //Debug.Log("ACd: " + GetComponent<PlayerController>().abilityCooldowns[GetComponent<PlayerController>().abilityNum]);
                    //Debug.Log("TT: " + target.tag);
                }
            }
        }
        if (invisibilityTimer <= timeInInvisibility)
        {
            invisibilityTimer += Time.deltaTime;
        }
        if (invisibilityTimer >= timeInInvisibility && useInvisibility)
        {
            if (invisibilityTarget != null)
            {
                invisibilityTarget.GetComponent<PlayerController>().isInvisible = false;
                invisibilityTarget = null;
                invisibilityActive = false;
                useInvisibility = false;
                if (target != null)
                {
                    target = null;
                }
            }
        }
    }
}
