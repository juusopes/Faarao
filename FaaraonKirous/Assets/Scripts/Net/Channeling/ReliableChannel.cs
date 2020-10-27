using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using Priority_Queue;
using System.Linq;

public class ReliableChannel : IChannel
{
    private readonly Connection _connection;

    // Outgoing sequencing
    private readonly SimplePriorityQueue<int, long> _timeoutHeap = new SimplePriorityQueue<int, long>();
    private readonly Dictionary<int, PendingOutgoingPacket> _sendSequencer = new Dictionary<int, PendingOutgoingPacket>();
    private readonly HashSet<int> _ackBuffer = new HashSet<int>();
    private readonly Queue<Packet> _pendingSends = new Queue<Packet>();
    private int _lastOutgoingSequence;  // last frame transmitted
    private int _outgoingLowestAckedSequence;  // last frame acknowledged
    // Current window size? How many frame can I still send. Thus doesn't include buffered frames
    private readonly object _sendLock = new object();

    // Incoming sequencing
    private readonly Dictionary<int, Packet> _receiveBuffer = new Dictionary<int, Packet>();
    private int _incomingLowestHandledSequence;
    private readonly object _receiveLock = new object();
    // TODO: ACK contains sequence number of next expected DATA frame?

    public ReliableChannel(Connection connection)
    {
        _connection = connection;
        _lastOutgoingSequence = -1;
        _outgoingLowestAckedSequence = -1;
        _incomingLowestHandledSequence = -1;
    }

    public static int CalculateDistance(int top, int bottom)
    {
        if (bottom <= top)
        {
            return top - bottom;
        }
        else
        {
            return (Constants.maxSequenceNumber - bottom) + top;
        }
    }

    public void BeginSendPacket(Packet packet)
    {
        lock (_sendLock)
        {
            if (_pendingSends.Count == 0 && CalculateDistance(_lastOutgoingSequence, _outgoingLowestAckedSequence) < Constants.windowSize)
            {
                SendPacketWithoutPending(packet);
            }
            else
            {
                //Debug.Log($"Packet sent to pending queue. Packets in queue {_pendingSends.Count}");
                _pendingSends.Enqueue(packet);
            }
        }
    }

    private void SendPacketWithoutPending(Packet packet)
    {
        lock (_sendLock)
        {
            // Add sequence number
            _lastOutgoingSequence = (_lastOutgoingSequence + 1) % Constants.maxSequenceNumber;
            packet.InsertInt(_lastOutgoingSequence);

            // Packet is not an ACK
            packet.InsertBool(false);

            // Add to sequencer for possible resend
            _sendSequencer.Add(_lastOutgoingSequence, new PendingOutgoingPacket(packet));

            // Add timeout
            _timeoutHeap.Enqueue(_lastOutgoingSequence,
                DateTime.Now.Ticks + (long)((long)_connection.Ping * TimeSpan.TicksPerMillisecond * Constants.resendMultiplier));

            //Debug.Log($"Sending packet {_lastOutgoingSequence}");
            _connection.SendPacket(packet, ChannelType.Reliable);
        }
    }

    private void SendACK(int sequence)
    {
        using (var packet = new Packet())
        {
            // Insert sequence
            packet.InsertInt(sequence);

            // Packet is an ACK
            packet.InsertBool(true);

            //Debug.Log($"Sending Ack {sequence}");
            _connection.SendPacket(packet, ChannelType.Reliable);
        }
    }

    private void HandleACK(Packet packet)
    {
        lock (_sendLock)
        {
            int sequence = packet.ReadInt();

            if (_sendSequencer.ContainsKey(sequence))
            {
                _timeoutHeap.Remove(sequence);
                _sendSequencer.Remove(sequence);
                int nextToHandle = (_outgoingLowestAckedSequence + 1) % Constants.maxSequenceNumber;
                if (sequence == nextToHandle)
                {
                    _outgoingLowestAckedSequence = nextToHandle;
                    //Debug.Log($"Ack received in order {sequence}, _outgoingLowestAckedSequence = {_outgoingLowestAckedSequence}");
                }
                else
                {
                    _ackBuffer.Add(sequence);
                    //Debug.Log($"Ack received out of order {sequence} and buffered");
                }
                
            }
            else
            {
                //Debug.Log($"Incorrect ack received {sequence}");
            }
        }
    }

    public void BeginHandlePacket(Packet packet)
    {
        // Handle ACK message
        bool isAck = packet.ReadBool();
        if (isAck) 
        {
            HandleACK(packet);
            return;
        }

        // Send ACK
        int sequence = packet.ReadInt();
        SendACK(sequence);

        lock (_receiveLock)
        {
            // Ignore out of window packets
            int windowMax = (_incomingLowestHandledSequence + Constants.windowSize) % Constants.maxSequenceNumber;
            // Current window is normal
            if (_incomingLowestHandledSequence < windowMax)
            {
                if (sequence < _incomingLowestHandledSequence + 1 || windowMax < sequence)
                {
                    //Debug.Log($"Out of window received(1) {sequence}, {_incomingLowestHandledSequence + 1}, {windowMax}");
                    return;
                }
            }
            // Current window is wrapped
            else
            {
                if (windowMax < sequence && sequence < _incomingLowestHandledSequence + 1)
                {
                    //Debug.Log($"Out of window received(2) {sequence}, {_incomingLowestHandledSequence + 1}, {windowMax}");
                    return;
                }
            }

            // Ignore duplicates
            if (_receiveBuffer.ContainsKey(sequence))
            {
                //Debug.Log($"Duplicate received {sequence}");
                return;
            }


            // Try to handle immediately
            if (sequence == (_incomingLowestHandledSequence + 1) % Constants.maxSequenceNumber)
            {
                //Debug.Log($"Handling packet immediately {sequence}");
                _incomingLowestHandledSequence = (_incomingLowestHandledSequence + 1) % Constants.maxSequenceNumber;
                _connection.HandlePacket(packet);
                return;
            }

            // Add packet to buffer
            //Debug.Log($"Putting packet to buffer {sequence}");
            _receiveBuffer.Add(sequence, packet);
        }
    }

    public void InternalUpdate(out bool timeout)
    {
        lock (_sendLock)
        {
            // Handle buffered acknowledgements
            int nextToHandle;
            while (_ackBuffer.Contains(nextToHandle = (_outgoingLowestAckedSequence + 1) % Constants.maxSequenceNumber))
            {
                _outgoingLowestAckedSequence = nextToHandle;
                _ackBuffer.Remove(nextToHandle);
            }

            // Resend packets
            while (0 < _timeoutHeap.Count)
            {
                int firstSequence = _timeoutHeap.First;
                long nextResendTime = _timeoutHeap.GetPriority(firstSequence);
                long timeNow = DateTime.Now.Ticks;
                
                if (nextResendTime < timeNow)
                {
                    PendingOutgoingPacket pendingOutgoingPacket = _sendSequencer[firstSequence];
                    if (Constants.maxResends <= pendingOutgoingPacket.Attempts)
                    {
                        timeout = true;
                        return;
                    }
                    //Debug.Log($"{(long)((long)_connection.Ping * TimeSpan.TicksPerMillisecond * Constants.resendMultiplier)}");
                    pendingOutgoingPacket.Attempts++;
                    _timeoutHeap.UpdatePriority(firstSequence, 
                        timeNow + (long)((long)_connection.Ping * TimeSpan.TicksPerMillisecond * Constants.resendMultiplier));
                    _connection.SendPacket(pendingOutgoingPacket.Packet, ChannelType.Reliable, false);
                }
                else
                {
                    break;
                }
            }

            // Send pending packets
            while (0 < _pendingSends.Count && CalculateDistance(_lastOutgoingSequence, _outgoingLowestAckedSequence) < Constants.windowSize) 
            {
                //Debug.Log($"Sending a pending packet. Packets before dequeue in queue {_pendingSends.Count}");
                SendPacketWithoutPending(_pendingSends.Dequeue());
            }
        }

        lock (_receiveLock)
        {
            // Handle buffered packets
            int nextToHandle;
            while (_receiveBuffer.ContainsKey(nextToHandle = (_incomingLowestHandledSequence + 1) % Constants.maxSequenceNumber))
            {
                //Debug.Log($"Handling a buffered packet {nextToHandle}");
                _incomingLowestHandledSequence = nextToHandle;
                _connection.HandlePacket(_receiveBuffer[_incomingLowestHandledSequence]);
                _receiveBuffer.Remove(_incomingLowestHandledSequence);
            }
        }

        timeout = false;
    }
}
