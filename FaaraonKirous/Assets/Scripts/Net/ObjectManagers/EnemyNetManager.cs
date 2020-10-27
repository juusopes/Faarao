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

    }

    public override void HandleSync(Packet packet)
    {
        base.HandleSync(packet);

    }
}
