 using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/AIClass", order = 1)]
public class AIClass : ScriptableObject
{
    [Range(0.01f, 500)]
    public float searchTurnSpeed;
    [Range(0, 1000)]
    public float searchingDuration;
    [Range(0, 1000)]
    public float sightRange;
    [Range(0, 100)]
    public float sightSpeed;
    [Range(0, 180)]
    public float fov;
    [Range(0,180)]
    public float alertHorizontalArc;
    [Range(0, 100)]
    public float navStoppingDistance;
    public LineMaterials lm;

}
