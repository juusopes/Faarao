using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterObjectManager : DynamicObjectManager
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
        packet.Write(DeathScript.hp);
    }

    public override void HandleSync(Packet packet)
    {
        base.HandleSync(packet);
        DeathScript.isDead = packet.ReadBool();
        DeathScript.hp = packet.ReadFloat();
    }
}
