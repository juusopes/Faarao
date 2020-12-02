using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathSentence : MonoBehaviour
{
    public bool currentDeath;
    public GameObject deathCanvas;

    private InGameMenu canvas;
    private PlayerController player;

    private int randNum;

    // Start is called before the first frame update
    void Start()
    {
        currentDeath = false;
        deathCanvas.SetActive(false);
        canvas = GameObject.Find("Canvas").GetComponent<InGameMenu>();
        randNum = Random.Range(1, 37);
        player = GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!player.IsDead)
        {
            randNum = Random.Range(1, 37);
            currentDeath = false;
        } else
        {
            currentDeath = true;
        }
        Debug.Log(canvas.gameObject.GetComponent<UnitInteractions>().gameOver);
        if (!canvas.menuActive && !canvas.gameObject.GetComponent<UnitInteractions>().gameOver)
        {
            if (deathCanvas.activeSelf == false)
            {
                for (int i = 0; i < deathCanvas.transform.childCount; i++)
                {
                    var child = deathCanvas.transform.GetChild(i).gameObject;
                    if (child != null)
                    {
                        currentDeath = false;
                        child.SetActive(false);
                    }
                }
            } else
            {
               deathCanvas.SetActive(true);
            }

            if (!currentDeath)
            {
                deathCanvas.transform.GetChild(0).gameObject.SetActive(true);
                deathCanvas.transform.GetChild(randNum).gameObject.SetActive(true);
                currentDeath = true;
            }
        } else
        {
            deathCanvas.SetActive(false);
        }
    }
}
