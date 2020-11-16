using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyObjectManager : CharacterObjectManager
{
    public Character Character { get; private set; }
    public float LatestDetectionConeTimestamp { get; set; } = 0;
    public bool AcceptingDetectionConeUpdates { get; set; } = false;

    protected override void InitComponents()
    {
        base.InitComponents();
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
        packet.Write((byte)Character.CurrentStateOption);

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
