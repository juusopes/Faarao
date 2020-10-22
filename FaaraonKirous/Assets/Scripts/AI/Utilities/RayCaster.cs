using System;
using System.Collections.Generic;
using UnityEngine;


public static class RayCaster
{
    public static string PLAYER_TAG = "Player";
    public static LayerMask layerDefault = 1 << 0;
    public static LayerMask layerTrans = 1 << 1;
    public static LayerMask layerIgnoreRay = 1 << 2;
    public static LayerMask layerPlayer = 1 << LayerMask.NameToLayer("Player");
    public static LayerMask layerEnemy = 1 << LayerMask.NameToLayer("Enemy");
    public static LayerMask layerLadder = 1 << LayerMask.NameToLayer("Raycast/Ladder");
    public static LayerMask layerDistraction = 1 << LayerMask.NameToLayer("Raycast/Distraction");
    public static LayerMask layerEditor = 1 << LayerMask.NameToLayer("EditorOnly");

    public static  LayerMask defaultLayerMask = ~(1 << 2);   //~() means all other than what is inside


    public static int LayerListStructuses = layerDefault;
    public static int LayerListDefaultIgnore = layerIgnoreRay | layerEditor;
    public static int LayerListCharacters = layerPlayer | layerEnemy;
    public static int LayerListRaycastable = layerLadder | layerDistraction;
    public static int LayerListObjects = LayerListCharacters | LayerListRaycastable;

    //public static LayerMask defaultLayerMaskMinusPlayer = ~(1 << 2 | 1 << layerPlayer);
    //public static LayerMask defaultLayerMaskMinusCharacters = ~(1 << 2 | 1 << layerPlayer | 1 << layerEnemy);
    public static LayerMask ladderLayerMask = layerLadder;
    public static LayerMask viewConeLayerMask = ~(LayerListDefaultIgnore | LayerListObjects);
    public static LayerMask distractionLayerMask = LayerListStructuses | layerDistraction;
    public static LayerMask playerDetectLayerMask = ~(LayerListDefaultIgnore | layerTrans | layerEnemy | LayerListRaycastable);


    public static RaycastHit ToTarget(GameObject start, GameObject target, float range, LayerMask layerMask)
    {
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
        if (hit.collider == null)
            return false;
        //Debug.Log(hit.collider.gameObject.name);

        if (string.IsNullOrEmpty(tag) && hit.collider)
            return true;
        return hit.collider.CompareTag(tag);
    }
}
