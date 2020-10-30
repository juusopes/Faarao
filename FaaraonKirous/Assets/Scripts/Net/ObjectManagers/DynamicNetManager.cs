﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicNetManager : ObjectNetManager
{
    public long LatestTransformTimestamp { get; set; } = 0;

    protected override void FixedUpdate()
    {
        base.FixedUpdate();  // TODO: Are we calling fixed update a second time?
        if (NetworkManager._instance.ShouldSendToClient) ServerSend.UpdateObjectTransform(List, Id, Transform.position, Transform.rotation);
    }
}
