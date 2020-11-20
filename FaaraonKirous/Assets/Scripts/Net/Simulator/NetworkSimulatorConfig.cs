using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct NetworkSimulatorConfig
{
    public float DropPercentage { get; set; }

    public int MinLatency { get; set; }

    public int MaxLatency { get; set; }
}
