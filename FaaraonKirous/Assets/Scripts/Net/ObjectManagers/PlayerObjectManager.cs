using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerObjectManager : CharacterObjectManager
{
    public PlayerController PlayerController { get; private set; }

    private int? _controller = null;
    public int? Controller
    {
        get
        {
            return _controller;
        }
        set
        {
            if (value.HasValue)
            {
                // Select
                GameManager._instance.Players[value.Value].ControlledCharacter = Type;
                PlayerController.IsActivePlayer = true;

                // If controller is me
                if (value.Value == GameManager._instance.CurrentPlayerId)
                {
                    PlayerController.IsCurrentPlayer = true;
                }
            }
            else
            {
                // Unselect
                if (_controller.HasValue)
                {
                    GameManager._instance.Players[(int)_controller].ControlledCharacter = null;
                }
                PlayerController.IsActivePlayer = false;

                // If controller was me
                if (PlayerController.IsCurrentPlayer)
                {
                    PlayerController.IsCurrentPlayer = false;
                }
            }

            _controller = value;
        }
    }

    protected override void Awake()
    {
        IsUnique = true;
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

        // Movement states
        packet.Write(PlayerController.IsRunning);
        packet.Write(PlayerController.IsCrouching);

        // Controller
        if (Controller.HasValue)
        {
            packet.Write(Controller.Value);
        }
        else
        {
            packet.Write(Constants.noId);
        }
    }

    public override void HandleSync(Packet packet)
    {
        base.HandleSync(packet);

        // Movement states
        PlayerController.IsRunning = packet.ReadBool();
        PlayerController.IsCrouching = packet.ReadBool();

        // Controller
        int controller = packet.ReadInt();
        if (NetworkManager._instance.IsHost)
        {
            Controller = null;
        }
        else
        {
            if (controller == Constants.noId)
            {
                Controller = null;
            }
            else
            {
                Controller = controller;
            }
        }
    }

    public override void WriteState(Packet dataPacket)
    {
        base.WriteState(dataPacket);
    }

    public override void ReadState(Packet dataPacket)
    {
        base.ReadState(dataPacket);
        PlayerController.navMeshAgent.Warp(Transform.position);
        PlayerController.Stay();
    }

    public override void ResetObject()
    {
        base.ResetObject();

        // Controller must be reset so that we don't get any bugs
        _controller = null;
    }
}
