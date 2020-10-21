using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Distraction : MonoBehaviour
{
    [SerializeField]
    private DistractionClass distractionClass;

    void Start()
    {
        Assert.IsNotNull(distractionClass, "No distraction class set");

        Destroy(this.gameObject, distractionClass.destroyTime);
    }
}
