using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Distraction", order = 1)]
public class DistractionClass : ScriptableObject
{
    [Tooltip("How AI detect distraction.")]
    public DetectionType detectionType;
    [Tooltip("How AI gets distracted by this")]
    public DistractionType distractionType;
    [Tooltip("How long the effect affects AI.")]
    public float effectTime;
    [Tooltip("How long till the gameObject is destroyed. Does not affect effect time at all.")]
    public float destroyTime;
}
