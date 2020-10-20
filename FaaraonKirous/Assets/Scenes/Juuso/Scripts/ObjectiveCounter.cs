using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveCounter : MonoBehaviour
{
    public GameObject objective1;
    public Text objective1text;

    public GameObject objective2;
    public Text objective2text;

    public GameObject objective3;
    public Text objective3text;

    public GameObject objective4;
    public Text objective4text;

    public GameObject objective5;
    public Text objective5text;

    // Start is called before the first frame update
    void Start()
    {
        objective1.SetActive(false);
        objective2.SetActive(false);
        objective3.SetActive(false);
        objective4.SetActive(false);
        objective5.SetActive(false);

        objective1text.text = "step on platform";
        objective2text.text = "Get to souteast corner";
        objective3text.text = "Get to northeast corner";
        objective4text.text = "banana for scale";
        objective5text.text = "fish for sale ipsum lorem mitelie meneekää testinä että saa toiselle riville asti asdasdasd";

        AddObjectives(3);
    }

    // Update is called once per frame
    void Update()
    {
    }

    private int AddObjectives(int objCount)
    {
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

    void OnCollisionEnter(Collision collision)
    {
        print("asdf");
        if (collision.gameObject.CompareTag("Player"))
        {
            print("the what now");
        }
    }
}