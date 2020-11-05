using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ObjController : MonoBehaviour
{

    public GameObject objective1, objective2, objective3, objective4, objective5;
    public int playersInside;

    public Objectivecounter2 objectivecounter1, objectivecounter2, objectivecounter3, objectivecounter4, objectivecounter5, endPointer;

    public bool objective1Done, objective2Done, objective3Done, objective4Done, objective5Done, inEndPoint;

    // Start is called before the first frame update
    void Start()
    {
        objective1.SetActive(false);
        objective2.SetActive(false);
        objective3.SetActive(false);
        objective4.SetActive(false);
        objective5.SetActive(false);

        Scene scene = SceneManager.GetActiveScene();

        if(scene.name == "Level 1")
        {
            AddObjectives(3);
        }

        else if (scene.name == "Level 2")
        {
            AddObjectives(4);
        }

    }

    public void Update()
    {
        objective1Done = objectivecounter1.objective1Done;
        objective2Done = objectivecounter2.objective2Done;
        objective3Done = objectivecounter3.objective3Done;
        objective4Done = objectivecounter4.objective4Done;
        objective5Done = objectivecounter5.objective5Done;
        inEndPoint = endPointer.inEndPoint;
        playersInside = endPointer.playersInside;

        //Jos kaikki objectivet tehty ja molemmat pelaajat ovat loppukohdassa
        if (objective1Done && objective2Done && objective3Done && inEndPoint && playersInside == 2)
        {
            //nyt toistaiseksi kovakoodattuna.
            SceneManager.LoadScene("Level 2");
        }
    }

    public int AddObjectives(int objCount)
    {
        //Adds objectives in scene
        List<int> objectives = new List<int>();

        for (int i = 0; i < objCount; i++)
        {
            objectives.Add(i);

            if (objectives.Contains(0))
            {
                objective1.SetActive(true);
            }
            if (objectives.Contains(1))
            {
                objective2.SetActive(true);
            }
            if (objectives.Contains(2))
            {
                objective3.SetActive(true);
            }
            if (objectives.Contains(3))
            {
                objective4.SetActive(true);
            }
            if (objectives.Contains(4))
            {
                objective5.SetActive(true);
            }
        }

        return objCount;
    }
}