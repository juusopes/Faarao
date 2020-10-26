using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class AutoDestroy : MonoBehaviour
{
    [SerializeField]
    private float destroyTime = 5f;

    void Start()
    {
        Destroy(this.gameObject, destroyTime);
    }
}
