using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
public class ColliderDebugger : MonoBehaviour
{
    [ContextMenu("Find All Mesh Colliders")]
    public void FindMeshColliders()
    {
        List<GameObject> objectsInLayer = GetObjectsInLayer(15);
        List<GameObject> objectsWithMeshCollider = GetObjectsWithMeshCollider(objectsInLayer);
        PrintList(objectsWithMeshCollider);
    }

    [ContextMenu("Find All Without Colliders")]
    public void FindWithoutColliders()
    {
        List<GameObject> objectsInLayer = GetObjectsInLayer(15);
        List<GameObject> objectsWithoutCollider = GetObjectsWithoutCollider(objectsInLayer);
        PrintList(objectsWithoutCollider);
    }


    [ExecuteInEditMode]
    private static void PrintList(List<GameObject> objects)
    {
        Debug.Log("Printing object list");
        if (objects.Count == 0)
        {
            Debug.Log("Nothing found in the list");
        }
        // iterate root objects and do something
        for (int i = 0; i < objects.Count; ++i)
        {
            GameObject gameObject = objects[i];
            Debug.Log("Found object: " + gameObject.name, gameObject);
        }
    }

    [ExecuteInEditMode]
    private static List<GameObject> GetObjectsWithMeshCollider(List<GameObject> objects)
    {
        Debug.Log("Get all objects with mesh collider");
        var ret = new List<GameObject>();
        // iterate root objects and do something
        for (int i = 0; i < objects.Count; ++i)
        {
            GameObject gameObject = objects[i];
            if (gameObject.GetComponent<MeshCollider>() != null)
            {
                ret.Add(gameObject);
            }
        }
        return ret;
    }

    [ExecuteInEditMode]
    private static List<GameObject> GetObjectsWithoutCollider(List<GameObject> objects)
    {
        Debug.Log("Get all objects with mesh collider");
        var ret = new List<GameObject>();
        // iterate root objects and do something
        for (int i = 0; i < objects.Count; ++i)
        {
            GameObject gameObject = objects[i];
            if (gameObject.GetComponent<Collider>() == null)
            {
                ret.Add(gameObject);
            }
        }
        return ret;
    }

    [ExecuteInEditMode]
    private static List<GameObject> GetObjectsInLayer(int layer)
    {
        Debug.Log("Get all objects in layer");
        // get root objects in scene
        GameObject[] rootObjects = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];
        var ret = new List<GameObject>();
        // iterate root objects and do something
        for (int i = 0; i < rootObjects.Length; ++i)
        {
            GameObject gameObject = rootObjects[i];
            if (gameObject.layer == layer)
            {
                ret.Add(gameObject);
                //Debug.Log(gameObject.name);
            }
        }
        return ret;
    }
}
