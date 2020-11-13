using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNetManager : CharacterNetManager
{
    public PlayerController PlayerController { get; private set; }

    protected override void Awake()
    {
        IsStatic = true;
        base.Awake();
    }

    protected override void InitComponents()
    {
        base.InitComponents();
        PlayerController = GetComponent<PlayerController>();
    }

    protected override void AddToGameManager()
    {
        base.AddToGameManager();
        GameManager._instance.AddPlayerCharacter(this);
    }

    public override void SendSync(Packet packet)
    {
        base.SendSync(packet);
        packet.Write(PlayerController.IsRunning);
        packet.Write(PlayerController.IsCrouching);
    }

    public override void HandleSync(Packet packet)
    {
        base.HandleSync(packet);
        PlayerController.IsRunning = packet.ReadBool();
        PlayerController.IsCrouching = packet.ReadBool();
    }
}
