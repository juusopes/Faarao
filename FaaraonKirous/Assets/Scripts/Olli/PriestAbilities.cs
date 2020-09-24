using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PriestAbilities : MonoBehaviour
{
    private PlayerController pharaoh;
    //Telekinesis
    private GameObject target;
    private LevelController levelControl;
    private bool telekinesisActive;
    private bool useTeleknesis;
    private float telekinesisTimer;
    private Vector3 telekinesisHeight;
    private Vector3 playerSavePos;

    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {   
        Telekinesis();
    }

    private void Initialize()
    {
        levelControl = GameObject.FindGameObjectWithTag("LevelController").GetComponent<LevelController>();
        telekinesisActive = true;
        telekinesisTimer = 5;
    }

    public void Telekinesis()
    {
        //TempSetActive
        if (GetComponent<PlayerController>().abilityIsActive)
        {
            telekinesisActive = true;
        }

        //TelekinesisSpell
        if (levelControl.targetObject != null)
        {
            target = levelControl.targetObject;
        }
        if (target != null) {
            Debug.Log("NoNullTarget");
            if (telekinesisActive)
            {
                Debug.Log(target);
                if (Input.GetKeyDown(KeyCode.Mouse1))
                {
                    Debug.Log("TeleTimer0");
                    telekinesisTimer = 0;
                    useTeleknesis = true;
                    target.GetComponent<Rigidbody>().isKinematic = true;
                    GetComponent<PlayerController>().abilityIsActive = false;
                }
                if (telekinesisTimer <= 4)
                {
                    telekinesisTimer += Time.deltaTime;
                }
                if (telekinesisTimer < 1)
                {
                    float yAxisValue = 0.01f;
                    target.transform.Translate(new Vector3(0, yAxisValue, 0));
                    Debug.Log("Up");
                }
                else if (telekinesisTimer > 1 && telekinesisTimer < 2)
                {
                    telekinesisHeight = target.transform.position;
                    playerSavePos = transform.position;
                    telekinesisTimer = 2;
                    Debug.Log("Side");
                }
                else if (telekinesisTimer >= 2 && telekinesisTimer < 4)
                {
                    target.transform.Translate((playerSavePos - target.transform.position) * (Time.deltaTime * 0.5f));
                    target.transform.position = new Vector3(target.transform.position.x, telekinesisHeight.y, target.transform.position.z);
                }
                if (telekinesisTimer >= 4 && useTeleknesis)
                {
                    target.GetComponent<Rigidbody>().isKinematic = false;
                    telekinesisActive = false;
                    useTeleknesis = false;
                    target = null;
                    Debug.Log("TeleKinesisInActive");
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
}
