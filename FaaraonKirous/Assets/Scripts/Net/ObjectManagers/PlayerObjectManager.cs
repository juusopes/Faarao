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
                if (value.Value == NetworkManager._instance.MyConnectionId)
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
            packet.Write(Constants.noConnectionId);
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
        if (controller == Constants.noConnectionId)
        {
            Controller = null;
        }
        else
        {
            Controller = controller;
        }
    }

    public override void Reset()
    {
        base.Reset();

        // Controller must be reset so that we don't get any bugs
        _controller = null;
    }
}
