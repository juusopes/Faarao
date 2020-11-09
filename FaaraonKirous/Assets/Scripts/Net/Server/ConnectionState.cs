using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Flags]
public enum ConnectionState
{
    None = 0,
    Synced = 1 << 0,
    Connected = 1 << 1,
    LevelLoaded = 1 << 2,
    All = None | Synced | Connected | LevelLoaded
}

public static class ConnectionStateExtensions
{
    public static ConnectionState ResetFlags(this ConnectionState state, ConnectionState flags)
    {
        return state &= ~flags;
    }

    public static ConnectionState SetFlags(this ConnectionState state, ConnectionState flags)
    {
        return state |= flags;
    }
}
