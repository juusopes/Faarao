using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectivesObjectManager : ObjectManager
{
    public ObjController ObjectiveController { get; private set; }

    protected override void Awake()
    {
        IsUnique = true;
        base.Awake();
    }

    protected override void InitComponents()
    {
        base.InitComponents();
        ObjectiveController = GetComponent<ObjController>();
    }

    public override void SendSync(Packet packet)
    {
        base.SendSync(packet);

        // Objectives done
        packet.Write(ObjectiveController.objectiveDone.Length);
        for (int i = 0; i < ObjectiveController.objectiveDone.Length; ++i)
        {
            packet.Write(ObjectiveController.objectiveDone[i]);
        }
    }

    public override void HandleSync(Packet packet)
    {
        base.HandleSync(packet);

        // Objectives done
        int objectiveDoneLength = packet.ReadInt();
        for (int i = 0; i < objectiveDoneLength; ++i)
        {
            ObjectiveController.objectiveDone[i] = packet.ReadBool();
        }
    }
}
