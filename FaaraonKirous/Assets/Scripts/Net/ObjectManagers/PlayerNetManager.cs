using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNetManager : DynamicNetManager
{
    protected override void Awake()
    {
        IsStatic = true;
        base.Awake();
    }

    protected override void AddToGameManager()
    {
        base.AddToGameManager();
        GameManager._instance.AddPlayerCharacter(this);
    }

    public override void SendSync(Packet packet)
    {
        base.SendSync(packet);
        // TODO: Not implemented
    }

    public override void HandleSync(Packet packet)
    {
        base.HandleSync(packet);
        // TODO: Not implemented
    }
}
