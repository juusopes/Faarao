using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
public class ColliderDebugger : ScriptableWizard
{
    [MenuItem("Henkka/Find All Mesh Colliders")]
    public static void FindMeshColliders()
    {
        List<GameObject> objectsInLayer = GetObjectsInLayer(15);
        List<GameObject> objectsWithMeshCollider = GetObjectsWithMeshCollider(objectsInLayer);
        PrintList(objectsWithMeshCollider);
    }

    [MenuItem("Henkka/Find All Without Colliders")]
    public static void FindWithoutColliders()
    {
        List<GameObject> objectsInLayer = GetObjectsInLayer(15);
        List<GameObject> objectsWithoutCollider = GetObjectsWithoutCollider(objectsInLayer);
        PrintList(objectsWithoutCollider);
    }
/*
    [MenuItem("Example/Select All of Tag...")]
    static void SelectAllOfTagWizard()
    {
        ScriptableWizard.DisplayWizard(
            "Select All of Tag...",
            typeof(SelectAllOfTag),
            "Make Selection");
    }
*/
    [MenuItem("Henkka/Select All of Tag HighestObject")]
    public static void SelectWithTag()
    {
        GameObject[] gos = GameObject.FindGameObjectsWithTag("HighestObject");
        Selection.objects = gos;
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
            //Get objects without collider and without child that has colliderer and renderer (sub models)
            if (gameObject.GetComponent<Collider>() == null && gameObject.GetComponentInChildren<Collider>() == null && gameObject.GetComponentInChildren<Renderer>() == null)
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
