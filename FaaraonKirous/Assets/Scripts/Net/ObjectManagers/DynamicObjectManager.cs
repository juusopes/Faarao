using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicObjectManager : ObjectManager
{
    public float LatestTransformTimestamp { get; set; } = 0;

    //private Packet _movementPacket = new Packet((int)ServerPackets.updateObjectTransform);

    protected override void FixedUpdate()
    {
        base.FixedUpdate();  // TODO: Are we calling fixed update a second time?
        if (NetworkManager._instance.ShouldSendToClient) ServerSend.UpdateObjectTransform(List, Id, Transform.position, Transform.rotation);
    }
}
