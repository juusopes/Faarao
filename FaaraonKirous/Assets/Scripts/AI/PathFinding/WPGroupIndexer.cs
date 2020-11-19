
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class WPGroupIndexer : MonoBehaviour
{
    private static WPGroupIndexer _instance;
    public static WPGroupIndexer Instance { get { return _instance; } }

    [SerializeField]
    private WaypointGroup[] waypointGroups;
    // Start is called before the first frame update
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
        if (!Application.isPlaying)
        {
            waypointGroups = FindObjectsOfType<WaypointGroup>();
            for (int i = 0; i < waypointGroups.Length; i++)
                waypointGroups[i].SetGroupId(i);
            // code here for Editor only
        }
#else
        if (Application.isPlaying)
        {
            Destroy(this.gameObject);
        }
#endif
    }
}
