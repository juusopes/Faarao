using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicObjectManager : ObjectManager
{
    public float LatestTransformTimestamp { get; set; } = 0;

    protected override void FixedUpdate()
    {
        base.FixedUpdate();  // TODO: Are we calling fixed update a second time?
        UpdateTransform();
    }

    protected virtual void UpdateTransform()
    {
        if (NetworkManager._instance.ShouldSendToClient)
        {
            ServerSend.UpdateObjectTransform(List, Id, Transform.position, Transform.rotation);
        }
    }
}
