using System;
using System.Collections.Generic;
using UnityEngine;


public static class RayCaster
{
    public static LayerMask defaultLayerMask = ~(1 << 2);
    public static LayerMask defaultLayerMaskMinusPlayer = ~(1 << 2 | 1 << LayerMask.NameToLayer("Player"));
    public static LayerMask defaultLayerMaskMinusCharacters = ~(1 << 2 | 1 << LayerMask.NameToLayer("Player") | 1 << LayerMask.NameToLayer("Enemy"));
    public static LayerMask ladderLayerMask = 1 << LayerMask.NameToLayer("EditorOnly/Ladder");
    public static LayerMask onlyZeroLayerMask = 1 << 0;
    public static RaycastHit ToTarget(GameObject start, GameObject target, float range)
    {
        RaycastHit hit;
        Vector3 enemyToTarget = target.transform.position - start.transform.position;

        //Debug.DrawRay(start.transform.position, enemyToTarget, Color.red, 1f);


        Physics.Raycast(start.transform.position, enemyToTarget, out hit, range, defaultLayerMask);

        return hit;
    }

    public static RaycastHit Forward(GameObject start, float range)
    {
        //Debug.DrawRay(start.transform.position, start.transform.forward * range, Color.green, 1f);

        RaycastHit hit;
        Physics.Raycast(start.transform.position, start.transform.forward, out hit, range, defaultLayerMask);

        return hit;
    }



    public static bool HitPlayer(RaycastHit hit)
    {
        if (hit.collider == null)
            return false;
        //Debug.Log(hit.collider.gameObject.name);
        return hit.collider.CompareTag("Player");
    }
}
