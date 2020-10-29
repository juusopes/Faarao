using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ObjController : MonoBehaviour
{
    //public int objectivesDone;

    public GameObject objective1;
    public Text objective1text;

    public GameObject objective2;
    public Text objective2text;

    public GameObject objective3;
    public Text objective3text;

    public int objCount;
    public int playersInside;

    public Objectivecounter2 objectivecounter1, objectivecounter2, objectivecounter3, endPointer;
    public bool objective1Done, objective2Done, objective3Done, inEndPoint;

    // Start is called before the first frame update
    void Start()
    {
        //objective1Done2 = false;

        objCount = 3;

        objective1.SetActive(false);
        objective2.SetActive(false);
        objective3.SetActive(false);
        //objective4.SetActive(false);
        //objective5.SetActive(false);

        objective1text.text = "step on platform";
        objective2text.text = "Get to souteast corner";
        objective3text.text = "Get to northeast corner";
        //objective4text.text = "banana for scale";
        //objective5text.text = "fish for sale ipsum lorem mitelie meneekää testinä että saa toiselle riville asti asdasdasd";


        AddObjectives(3);
    }

    public void Update()
    {
        objective1Done = objectivecounter1.objective1Done;
        objective2Done = objectivecounter2.objective2Done;
        objective3Done = objectivecounter3.objective3Done;
        inEndPoint = endPointer.inEndPoint;
        playersInside = endPointer.playersInside;

        //Jos kaikki objectivet tehty ja molemmat pelaajat ovat loppukohdassa
        if (objective1Done && objective2Done && objective3Done && inEndPoint && playersInside == 2)
        {
            print(playersInside + " loppu lähellä");

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
            //if (objectives.Contains(3))
            //{
            //    objective4.SetActive(true);
            //}
            //if (objectives.Contains(4))
            //{
            //    objective5.SetActive(true);
            //}
        }

        return objCount;
    }
}