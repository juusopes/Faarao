using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ChannelType : byte
{
    Unreliable,
    Reliable
}

public interface IChannel
{
    void BeginSendPacket(Packet packet);
    void BeginHandlePacket(Packet packet);

    void InternalUpdate(out bool timeout);
}
