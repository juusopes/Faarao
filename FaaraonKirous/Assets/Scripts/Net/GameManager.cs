using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager _instance;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    public Dictionary<int, EnemyNetworkManager> _enemies = new Dictionary<int, EnemyNetworkManager>();
    private int _enemyIdCounter = 0;
    [SerializeField]
    private GameObject _enemyClient;

    #region Enemy
    private void AddEnemy(int id, EnemyNetworkManager enemyNetworkManager)
    {
        _enemies.Add(id, enemyNetworkManager);
        enemyNetworkManager.Id = id;
    }

    public void EnemyCreated(EnemyNetworkManager enemyNetworkManager)
    {
        AddEnemy(_enemyIdCounter, enemyNetworkManager);

        if (Server.Instance.IsOnline)
        {
            ServerSend.EnemyCreated(Constants.DefaultConnectionId, _enemyIdCounter,
                enemyNetworkManager.Transform.position);
        }

        _enemyIdCounter++;
    }

    public void CreateEnemy(int id, Vector3 position)
    {
        GameObject newEnemy = Instantiate(_enemyClient, position, Quaternion.identity);
        AddEnemy(id, newEnemy.GetComponent<EnemyNetworkManager>());
    }

    public void UpdateEnemyTransform(int id, Vector3 position, Quaternion quaternion)
    {
        if (_enemies.ContainsKey(id))
        {
            _enemies[id].Transform.position = position;
            _enemies[id].Transform.rotation = quaternion;
        }
    }
    #endregion
}
