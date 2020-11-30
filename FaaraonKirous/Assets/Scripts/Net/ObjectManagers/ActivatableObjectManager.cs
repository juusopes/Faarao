using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivatableObjectManager : ObjectManager
{
    public ActivatorScript ActivatorScript { get; private set; }

    protected override void Awake()
    {
        IsPreIndexed = true;
        base.Awake();
    }

    protected override void InitComponents()
    {
        base.InitComponents();
        ActivatorScript = GetComponent<ActivatorScript>();
    }

    public override void SendSync(Packet packet)
    {
        base.SendSync(packet);
        packet.Write(ActivatorScript.activated);
    }

    public override void HandleSync(Packet packet)
    {
        base.HandleSync(packet);
        ActivatorScript.activated = packet.ReadBool();
    }
}
