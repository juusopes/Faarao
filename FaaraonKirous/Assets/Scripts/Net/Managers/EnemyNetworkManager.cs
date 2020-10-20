using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyNetworkManager : MonoBehaviour
{
    public int Id { get; set; }
    public Transform Transform { get; private set; }

    private void Awake()
    {
        Transform = transform;
    }
}