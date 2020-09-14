using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveCounter : MonoBehaviour
{
    public bool objective1;
    public bool objective2;
    public bool objective3;
    public bool objective4;
    public bool objective5;

    public int objectives;

    // Start is called before the first frame update
    void Start()
    {
        AddObjectives(5);

    }

    // Update is called once per frame
    void Update()
    {

    }

    private int AddObjectives(int objCount)
    {
        objectives = objCount;

        for (int i = 1; i <= objectives; i++)
        {
            print("objective" + i + " added");
        }

        return objectives;
    }
}