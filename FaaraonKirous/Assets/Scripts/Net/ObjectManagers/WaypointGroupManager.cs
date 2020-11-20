using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointGroupManager : ObjectManager
{
    public WaypointGroup WaypointGroup { get; private set; }

    protected override void Awake()
    {
        IsSyncable = false;
        IsPreIndexed = true;
        base.Awake();
    }

    protected override void InitComponents()
    {
        base.InitComponents();
        WaypointGroup = GetComponent<WaypointGroup>();
    }
}
