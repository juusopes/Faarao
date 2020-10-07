using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;

public enum WaypointType
{
    WalkPast,
    GuardForDuration,
    GuardForEver
}

public class Waypoint : MonoBehaviour
{
    [Header("Waypoint Type")]
    [Tooltip("What happens when this waypoint is reached.")]
    public WaypointType type;
    [Header("How long to guard")]
    [Tooltip("While guarding: How many seconds this waypoint will be guarded.")]
    [SerializeField][Range(0, 600)]
    private float guardDuration = 6;
    [Tooltip("While guarding: Random between duration-random to duration+random")]
    [SerializeField]
    private float guardDurationRandom = 0;
    [Header("How to guard")]
    [Tooltip("While guarding: How long it pauses between rotations")]
    [SerializeField][Range(0, 60)]
    private float waitDuration = 1;
    [Tooltip("While guarding: Random between duration-random to duration+random")]
    [SerializeField]
    private float waitDurationRandom = 0;
    [Tooltip("While guarding: How many degrees horizontally will be rotated from waypoint orientation to each direction.")]
    [SerializeField][Range(0, 180)]
    private float horizontalArc = 45;
    [Tooltip("While guarding: Random between horizontalArc-random to horizontalArc+random")]
    [SerializeField]
    private float horizontalArcRandom = 0;
    [Tooltip("While guarding: Speed of rotation")]
    [SerializeField][Range(10, 500)]
    private float rotationSpeed = 150;
    [Tooltip("While guarding: Random between rotationSpeed-random to rotationSpeed+random, beware to not get speed near zero")]
    [SerializeField]
    private float rotationSpeedRandom = 0;

    private void Awake()
    {
        if (type > 0 && rotationSpeed <= 0 || rotationSpeed <= rotationSpeedRandom )
            Debug.LogWarning("Seriously, you deserve sleep!");
    }

    public float GetGuardDuration()
    {
        return GetRandomised(guardDuration, guardDurationRandom);
    }
    public float GetHorizontalArc()
    {
        return GetRandomised(horizontalArc, horizontalArcRandom);
    }
    public float GetRotationSpeed()
    {
        return GetRandomised(rotationSpeed, rotationSpeedRandom);
    }

    public float GetWaitDuration()
    {
        return GetRandomised(waitDuration, waitDurationRandom);
    }

    private float GetRandomised(float field, float random)
    {
        return Mathf.Max(0, field + Random.Range(-random, random));
    }

    void OnDrawGizmos()
    {
        if (this.gameObject != null)
        {
            float vecLenght = 2f;
            Vector3 offset = new Vector3(0, 0.3f, 0);

            if (type == WaypointType.WalkPast)
            {
                DrawWalkPast(vecLenght, offset, Color.green);
            }
            else if(type == WaypointType.GuardForDuration)
            {
                DrawWalkPast(vecLenght, offset, Color.green);
                DrawHarc(vecLenght, offset, Color.red);
            }
            else
            {
                DrawHarc(vecLenght, offset, Color.magenta);
            }

            DrawConnections(new Vector3(0, 0.5f, 0), Color.yellow);
        }
    }

    private void DrawHarc(float lenght, Vector3 offset, Color color)
    {
        Quaternion rotation1 = Quaternion.AngleAxis(-horizontalArc, Vector3.up);
        Vector3 targetPos1 = rotation1 * transform.forward * lenght + transform.position;

        Quaternion rotation2 = Quaternion.AngleAxis(horizontalArc, Vector3.up);
        Vector3 targetPos2 = rotation2 * transform.forward * lenght + transform.position;

        Gizmos.color = color;
        Gizmos.DrawLine(transform.position + offset, targetPos1 + offset);
        Gizmos.DrawLine(transform.position + offset, targetPos2 + offset);
    }

    private void DrawWalkPast(float lenght, Vector3 offset, Color color)
    {
        Vector3 vectorInFront = transform.position + transform.forward * lenght;

        Gizmos.color = color;
        Gizmos.DrawLine(transform.position + offset, vectorInFront + offset);
    }

    private void DrawConnections(Vector3 offset, Color color)
    {
        int index = transform.GetSiblingIndex();
        Transform nextBrotherNode;
        if (transform.parent.childCount >= index + 2)
            nextBrotherNode = transform.parent.GetChild(index + 1);
        else
            return;

        Gizmos.color = color;
        Gizmos.DrawLine(transform.position + offset, nextBrotherNode.position);
    }
}

