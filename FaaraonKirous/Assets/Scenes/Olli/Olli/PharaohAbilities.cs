using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PharaohAbilities : MonoBehaviour
{
    private PlayerController priest;
    //Invisibility
    private GameObject target;
    private LevelController levelControl;
    private bool invisibilityActive;
    private bool useInvisibility;
    private float invisibilityTimer;

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
        invisibilityTimer = 5;
    }

    public void Invisibility()
    {
        if (GetComponent<PlayerController>().isActiveCharacter)
        {
            //TempSetActive
            if (GetComponent<PlayerController>().abilityIsActive)
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
            } else if (!useInvisibility)
            {
                target = null;
            }
            if (target != null)
            {
                if (invisibilityActive)
                {
                    if (Input.GetKeyDown(KeyCode.Mouse1))
                    {
                        target.GetComponent<PlayerController>().isInvisible = true;
                        invisibilityTimer = 0;
                        useInvisibility = true;
                        GetComponent<PlayerController>().abilityIsActive = false;
                    }
                }
            }
        }
        if (invisibilityTimer <= 4)
        {
            invisibilityTimer += Time.deltaTime;
        }
        if (invisibilityTimer >= 4 && useInvisibility)
        {
            target.GetComponent<PlayerController>().isInvisible = false;
            invisibilityActive = false;
            useInvisibility = false;
            target = null;
        }
    }
}
