using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterNetManager : DynamicNetManager
{
    public DeathScript DeathScript { get; private set; }

    protected override void InitComponents()
    {
        base.InitComponents();
        DeathScript = GetComponent<DeathScript>();
    }

    public override void SendSync(Packet packet)
    {
        base.SendSync(packet);
        packet.Write(DeathScript.isDead);
    }

    public override void HandleSync(Packet packet)
    {
        base.HandleSync(packet);
        DeathScript.isDead = packet.ReadBool();
    }
}
