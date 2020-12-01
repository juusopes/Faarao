#if UNITY_EDITOR
using ParrelSync;
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class PreIndexer : MonoBehaviour
{
    private static PreIndexer _instance;
    public static PreIndexer Instance { get { return _instance; } }

    [SerializeField]
    private ActivatableObjectManager[] activatableObjects;
    [SerializeField]
    private WaypointGroupManager[] waypointGroups;
    [SerializeField]
    private EnemyObjectManager[] enemies;
    

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }

#if UNITY_EDITOR 
        if (!ClonesManager.IsClone() && !Application.isPlaying)
        {
            UpdateIndexes();
        }
#else
        if (Application.isPlaying)
        {
            Destroy(this.gameObject);
        }
#endif
    }

#if UNITY_EDITOR
    [ContextMenu("Update Indexing")]
    private void UpdateIndexes()
    {
        // Preindex activatable objects
        activatableObjects = FindObjectsOfType<ActivatableObjectManager>();
        for (int i = 0; i < activatableObjects.Length; ++i)
        {
            activatableObjects[i].Id = i;
            EditorUtility.SetDirty(activatableObjects[i]);
        }

        // Preindex waypoints
        waypointGroups = FindObjectsOfType<WaypointGroupManager>();
        for (int i = 0; i < waypointGroups.Length; ++i)
        {
            waypointGroups[i].Id = i;
            EditorUtility.SetDirty(waypointGroups[i]);
        }

        // Preindex enemies
        enemies = FindObjectsOfType<EnemyObjectManager>();
        for (int i = 0; i < enemies.Length; ++i)
        {
            enemies[i].Id = i;
            EditorUtility.SetDirty(enemies[i]);
        }

        Debug.Log("Preindexed");
    }
#endif
}
