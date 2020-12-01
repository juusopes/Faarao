using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathSentence : MonoBehaviour
{
    public bool currentDeath;
    public GameObject deathCanvas;

    // Start is called before the first frame update
    void Start()
    {
        currentDeath = false;
        deathCanvas.SetActive(false);
    }

    // Update is called once per frame
    void Update()
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
        }

        if (!currentDeath)
        {
            deathCanvas.transform.GetChild(0).gameObject.SetActive(true);
            deathCanvas.transform.GetChild(Random.Range(1, 37)).gameObject.SetActive(true);
            currentDeath = true;
        }
    }
}
