using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyNetManager : ObjectNetManager
{
    public Character Character;

    protected override void Awake()
    {
        base.Awake();
        Character = GetComponent<Character>();
    }

    public override void SendSync(Packet packet)
    {
        base.SendSync(packet);
        // TODO: Not implemented

        // Sight impairments
        packet.Write(Character.impairedSightRange);
        packet.Write(Character.impairedFOV);

        // State
        packet.Write((byte)Character.CurrentStateIndicator);

        // Detection cone

        // ..
    }

    public override void HandleSync(Packet packet)
    {
        base.HandleSync(packet);
        // TODO: Not implemented

        // Sight impairments
        Character.impairedSightRange = packet.ReadBool();
        Character.impairedFOV = packet.ReadBool();

        // State
        Character.UpdateStateIndicator((StateOption)packet.ReadByte());

        // Detection cone

        // ..

    }
}
