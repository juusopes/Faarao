using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraObjectManager : ObjectManager
{
    protected override void Awake()
    {
        IsUnique = true;
        base.Awake();
    }
}
