using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeltaTimeTester
{
    public float start, end;
    
    public void Test()
    {
        float s, e;

        Debug.Log("Testing 1");
        s = Time.realtimeSinceStartup;
        for(int i = 0; i < 1000000; i++)
        {
            //test 1
        }
        e = Time.realtimeSinceStartup;
        e -= s;
        Debug.Log("Test 1 time: " + e.ToString("0,0.00000"));

        Debug.Log("Testing 2");
        s = Time.realtimeSinceStartup;
        for (int i = 0; i < 1000000; i++)
        {
            //test 2
        }
        e = Time.realtimeSinceStartup;
        e -= s;
        Debug.Log("Test 2 time: " + e.ToString("0,0.00000"));
    }

    public void StopWatch()
    {
        if (Mathf.Approximately(start, 0))
        {
            start = Time.realtimeSinceStartup;
        }
        else
        {
            end = Time.realtimeSinceStartup;
            float sw = end - start;
            start = 0;
            Debug.Log("Time test result: " + sw);
        }
    }
}
