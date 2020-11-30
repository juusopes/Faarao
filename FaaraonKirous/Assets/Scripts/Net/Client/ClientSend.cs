using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSend
{

    #region Core
    public static void ConnectionRequest()
    {
        Debug.Log("Sending request");

        var packet = new Packet((int)ClientPackets.connectionRequest);
        Client.Instance.BeginSendPacket(ChannelType.Reliable, packet);
    }

    public static void HeartbeatReceived(long timeStamp)
    {
        var packet = new Packet((int)ClientPackets.heartbeatReceived);
        packet.Write(timeStamp);
        Client.Instance.BeginSendPacket(ChannelType.Unreliable, packet);
    }

    public static void Disconnecting()
    {
        var packet = new Packet((int)ClientPackets.disconnecting);
        Client.Instance.BeginSendPacket(ChannelType.Unreliable, packet);
    }
    #endregion

    #region Load

    public static void SyncRequest()
    {
        var packet = new Packet((int)ClientPackets.syncRequest);
        Client.Instance.BeginSendPacket(ChannelType.Reliable, packet);
    }
    #endregion


    #region Abilities
    public static void AbilityUsed(AbilityOption ability, Vector3 position)
    {
        var packet = new Packet((int)ClientPackets.abilityUsed);
        packet.Write((byte)ability);
        packet.Write(position);

        Client.Instance.BeginSendPacket(ChannelType.Reliable, packet);
    }

    public static void EnemyPossessed(int id, Vector3 position)
    {
        var packet = new Packet((int)ClientPackets.enemyPossessed);
        packet.Write(id);
        packet.Write(position);

        Client.Instance.BeginSendPacket(ChannelType.Reliable, packet);
    }
    #endregion

    #region Player

    public static void SelectCharacterRequest(ObjectType character)
    {
        var packet = new Packet((int)ClientPackets.selectCharacterRequest);
        packet.Write((short)character);

        Client.Instance.BeginSendPacket(ChannelType.Reliable, packet);
    }

    public static void UnselectCharacterRequest()
    {
        var packet = new Packet((int)ClientPackets.unselectCharacterRequest);

        Client.Instance.BeginSendPacket(ChannelType.Reliable, packet);
    }

    public static void SetDestinationRequest(ObjectType character, Vector3 destination)
    {
        var packet = new Packet((int)ClientPackets.setDestinationRequest);
        packet.Write((short)character);
        packet.Write(destination);

        Client.Instance.BeginSendPacket(ChannelType.Reliable, packet);
    }


    public static void KillEnemy(int id)
    {
        var packet = new Packet((int)ClientPackets.killEnemy);
        packet.Write(id);

        Client.Instance.BeginSendPacket(ChannelType.Reliable, packet);
    }

    public static void Revive(int id)
    {
        var packet = new Packet((int)ClientPackets.revive);
        packet.Write(id);

        Client.Instance.BeginSendPacket(ChannelType.Reliable, packet);
    }

    public static void Crouching(ObjectType character, bool state)
    {
        var packet = new Packet((int)ClientPackets.crouching);
        packet.Write((short)character);
        packet.Write(state);

        Client.Instance.BeginSendPacket(ChannelType.Reliable, packet);
    }

    public static void Running(ObjectType character, bool state)
    {
        var packet = new Packet((int)ClientPackets.running);
        packet.Write((short)character);
        packet.Write(state);

        Client.Instance.BeginSendPacket(ChannelType.Reliable, packet);
    }

    public static void Stay(ObjectType character)
    {
        var packet = new Packet((int)ClientPackets.stay);
        packet.Write((short)character);

        Client.Instance.BeginSendPacket(ChannelType.Reliable, packet);
    }

    public static void Warp(ObjectType character, Vector3 position)
    {
        var packet = new Packet((int)ClientPackets.warp);
        packet.Write((short)character);
        packet.Write(position);

        Client.Instance.BeginSendPacket(ChannelType.Reliable, packet);
    }
    #endregion

    #region Activatable
    public static void ActivateObject(int id)
    {
        var packet = new Packet((int)ClientPackets.activateObject);
        packet.Write(id);

        Client.Instance.BeginSendPacket(ChannelType.Reliable, packet);
    }

    #endregion
}
