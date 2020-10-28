using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PendingOutgoingPacket
{
    public int Attempts { get; set; } = 1;  // first attempt has already been sent
    public Packet Packet { get; private set; }

    public PendingOutgoingPacket(Packet packet)
    {
        Packet = packet;
    }
}
