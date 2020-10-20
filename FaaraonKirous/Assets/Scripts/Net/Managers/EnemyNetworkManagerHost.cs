using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class EnemyNetworkManagerHost : EnemyNetworkManager
{
    private void Awake()
    {
        GameManager._instance.EnemyCreated(this);
    }

    private void FixedUpdate()
    {
        if (Server.Instance.IsOnline)
        {
            ServerSend.EnemyTransformUpdate(Constants.DefaultConnectionId, Id, Transform.position, Transform.rotation);
        }
    }
}
