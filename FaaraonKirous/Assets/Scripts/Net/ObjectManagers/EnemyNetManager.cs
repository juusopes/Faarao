using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyNetManager : ObjectNetManager
{
    private Character _character;


    protected override void Awake()
    {
        base.Awake();
        _character = GetComponent<Character>();
    }

    public override void SendSync(Packet packet)
    {
        base.SendSync(packet);

    }

    public override void HandleSync(Packet packet)
    {
        base.HandleSync(packet);

    }
}
