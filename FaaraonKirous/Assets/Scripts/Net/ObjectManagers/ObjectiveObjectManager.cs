using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveObjectManager : ObjectManager
{
    protected override void Awake()
    {
        IsPreIndexed = true;
        base.Awake();
    }

    public override void SendSync(Packet packet)
    {
        base.SendSync(packet);
        packet.Write(gameObject.activeSelf);
    }

    public override void HandleSync(Packet packet)
    {
        base.HandleSync(packet);
        gameObject.SetActive(packet.ReadBool());
    }
}
