using Priority_Queue;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class NetworkSimulator
{
    private struct OutgoingPacket
    {
        public long ScheduledTime { get; set; }
        public IPEndPoint EndPoint { get; set; }
        public Packet Packet { get; set; }
    }

    public delegate void SendDelegate(Packet packet, IPEndPoint endPoint);

    private readonly System.Random _rand = new System.Random();
    private readonly SimplePriorityQueue<OutgoingPacket, long> _packets = new SimplePriorityQueue<OutgoingPacket, long>();
    private readonly NetworkSimulatorConfig _config;
    private readonly SendDelegate _sendDelegate;
    private readonly object _lock = new object();

    public NetworkSimulator(NetworkSimulatorConfig config, SendDelegate sendDelegate)
    {
        _config = config;
        _sendDelegate = sendDelegate;
    }

    public void Add(Packet packet, IPEndPoint endPoint)
    {
        Packet newPacket = new Packet(packet.ReadBytes(packet.Length(), false));

        if (_rand.NextDouble() < (double)_config.DropPercentage)
        {
            // Packet dropped
            return;
        }

        lock (_lock)
        {
            long scheduledTime = DateTime.Now.Ticks
                + (TimeSpan.TicksPerMillisecond * _rand.Next(_config.MinLatency, _config.MaxLatency + 1));
            _packets.Enqueue(new OutgoingPacket()
            {
                EndPoint = endPoint,
                Packet = newPacket,
                ScheduledTime = scheduledTime
            }, scheduledTime);
        }
    }

    public void InternalUpdate()
    {
        lock (_lock)
        {
            while (0 < _packets.Count && _packets.First.ScheduledTime <= DateTime.Now.Ticks)
            {
                _sendDelegate(_packets.First.Packet, _packets.First.EndPoint);
                _packets.Dequeue();
            }
        }
    }

    // TODO: Add to disconnect
    public void Flush()
    {
        lock (_lock)
        {
            while (0 < _packets.Count)
            {
                _sendDelegate(_packets.First.Packet, _packets.First.EndPoint);
                _packets.Dequeue();
            }
        }
    }
}
