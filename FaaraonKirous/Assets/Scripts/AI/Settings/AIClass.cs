 using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/AIClass", order = 1)]
public class AIClass : ScriptableObject
{
    [Header("Detection")]
    [Tooltip("How far AI can hear.")]
    [Range(0, 100)]
    public float hearingRange = 10f;
    [Tooltip("How far AI can see.")]
    [Range(0, 1000)]
    public float sightRange = 25f;
    [Tooltip("How far AI can see if player is crouching.")]
    [Range(0, 1000)]
    public float sightRangeCrouching = 15f;
    [Tooltip("How far AI can see when sight is distracted.")]
    [Range(0, 1000)]
    public float impairedSightRange = 10f;
    [Tooltip("How fast AI sightline renderer/detection moves.")]
    [Range(0, 100)]
    public float sightSpeed = 10f;
    [Tooltip("How wide AI can see.")]
    [Range(0, 180)]
    public float fov = 90f;
    [Tooltip("How wide AI can see when sight is distracted.")]
    [Range(0, 180)]
    public float impairedFov = 30f;
    [Tooltip("How long AI will search/wait/inspect by default")]
    [Range(0, 1000)]
    public float searchingDuration = 5f;

    [Header("TODO")]
    [Tooltip("NO EFFECT ATM")]
    [Range(0.01f, 500)]
    public float searchTurnSpeed = 150f;
    [Tooltip("NO EFFECT ATM")]
    [Range(0,180)]
    public float alertHorizontalArc = 45;

    [Header("Navigation Agent")]
    [Range(0, 100)]
    public float navSpeed = 4f;
    [Range(0, 1000)]
    public float navAngularSpeed = 420f;
    [Range(0, 100)]
    public float navAcceleration = 16f;
    [Range(0, 100)]
    public float navStoppingDistance = 0.5f;
    [Tooltip("Radius of AI model.")]
    [Range(0.01f, 10)]
    public float navJumpHeight = 1f;

    [Header("Graphics")]
    [Tooltip("Radius of AI model.")]
    [Range(0.1f, 5)]
    public float modelRadius = 0.5f;
    public LineMaterials lm;


}
