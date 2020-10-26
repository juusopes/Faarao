using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Distraction : MonoBehaviour
{
    [SerializeField]
    private DistractionClass distractionClass = null;
    [HideInInspector]
    public AbilityOption option;

    //Nicer API, nothing else
    public DetectionType detectionType => distractionClass.detectionType;
    public DistractionType distractionType => distractionClass.distractionType;
    public float effectTime => distractionClass.effectTime;

    void Start()
    {
        Assert.IsNotNull(distractionClass, "No distraction class set");

        Destroy(this.gameObject, distractionClass.destroyTime);
    }
}
