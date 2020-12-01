using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyObjectManager : CharacterObjectManager
{
    public Character Character { get; private set; }
    public float LatestDetectionConeTimestamp { get; set; } = 0;
    public bool AcceptingDetectionConeUpdates { get; set; } = false;

    protected override void Awake()
    {
        IsPreIndexed = true;
        base.Awake();
    }

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

        // Animation state
        packet.Write((byte)Character.CurrentAnimationState);

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
        if (NetworkManager._instance.IsHost)
        {
            Character.SetState((StateOption)packet.ReadByte());
        }
        else
        {
            Character.UpdateStateIndicator((StateOption)packet.ReadByte());
        }

        // Animation state
        Character.SetAnimation((AnimationState)packet.ReadByte());

        // Detection cone

        // ..

    }

    public override void WriteState(Packet dataPacket)
    {
        base.WriteState(dataPacket);
        dataPacket.Write(Character.lastSeenPosition);
        dataPacket.Write(Character.navigator.CurrentWaypointIndex);
    }

    public override void ReadState(Packet dataPacket)
    {
        base.ReadState(dataPacket);
        Vector3 lastSeenPosition = dataPacket.ReadVector3();
        int currentWaypoint = dataPacket.ReadInt();
        Character.SaveLoaded(lastSeenPosition, currentWaypoint);
    }
}
