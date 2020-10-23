using System;
using System.Collections.Generic;
using UnityEngine;


public static class RayCaster
{
    public static readonly string PLAYER_TAG = "Player";
    public static readonly string DISTRACTION_TAG = "Distraction";

    public static readonly LayerMask layerDefault = 1;
    public static readonly LayerMask layerTrans = 1 << 1;
    public static readonly LayerMask layerIgnoreRay = 1 << 2;
    public static readonly LayerMask layerPlayer = 1 << LayerMask.NameToLayer("Player");
    public static readonly LayerMask layerEnemy = 1 << LayerMask.NameToLayer("Enemy");
    public static readonly LayerMask layerLadder = 1 << LayerMask.NameToLayer("Raycast/Ladder");
    public static readonly LayerMask layerDistraction = 1 << LayerMask.NameToLayer("Raycast/Distraction");
    public static readonly LayerMask layerEditor = 1 << LayerMask.NameToLayer("EditorOnly");

    public static readonly LayerMask defaultLayerMask = ~(1 << 2);   //~() means all other than what is inside

    public static readonly int LayerListStructures = layerDefault;
    public static readonly int LayerListDefaultIgnore = layerIgnoreRay | layerEditor;
    public static readonly int LayerListCharacters = layerPlayer | layerEnemy;
    public static readonly int LayerListRaycastable = layerLadder | layerDistraction;
    public static readonly int LayerListObjects = LayerListCharacters | LayerListRaycastable;


    //public static readonly LayerMask defaultLayerMaskMinusPlayer = ~(1 << 2 | 1 << layerPlayer);
    //public static readonly LayerMask defaultLayerMaskMinusCharacters = ~(1 << 2 | 1 << layerPlayer | 1 << layerEnemy);
    public static readonly LayerMask ladderLayerMask = layerLadder;
    public static readonly LayerMask viewConeLayerMask = ~(LayerListDefaultIgnore | LayerListObjects);
    public static readonly LayerMask distractionLayerMask = LayerListStructures | layerDistraction;
    public static readonly LayerMask playerDetectLayerMask = LayerListStructures | layerPlayer;

    public static RaycastHit ToTarget(GameObject start, GameObject target, float range, LayerMask layerMask)
    {
        Debug.Log(System.Convert.ToString(layerMask));
        RaycastHit hit;
        Vector3 startToTarget = target.transform.position - start.transform.position;
        //Debug.DrawRay(start.transform.position, startToTarget, Color.red, 1f);
        Physics.Raycast(start.transform.position, startToTarget, out hit, range, layerMask);

        return hit;
    }

    public static RaycastHit Forward(GameObject start, float range)
    {
        //Debug.DrawRay(start.transform.position, start.transform.forward * range, Color.green, 1f);
        RaycastHit hit;
        Physics.Raycast(start.transform.position, start.transform.forward, out hit, range, defaultLayerMask);

        return hit;
    }

    public static bool HitObject(RaycastHit hit, string tag = "")
    {
        //Debug.Log(hit.collider);
        if (hit.collider == null)
            return false;
        //Debug.Log(hit.collider.gameObject.name);

        if (string.IsNullOrEmpty(tag) && hit.collider)
            return true;
        return hit.collider.CompareTag(tag);
    }
}
