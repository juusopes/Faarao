using System;
using UnityEngine;
using UnityEngine.Assertions;

public class SomeClass : MonoBehaviour
{

}

public class DistractionSpawner : MonoBehaviour
{
    private static DistractionSpawner _instance;
    public static DistractionSpawner Instance { get { return _instance; } }

    [SerializeField] private GameObject blindingLight = null;
    [SerializeField] private GameObject insectSwarm = null;
    [SerializeField] private GameObject noiseToGoTo = null;
    [SerializeField] private GameObject noiseToLookAt = null;
    [SerializeField] private GameObject sightToGoTo = null;
    [SerializeField] private GameObject sightToLookAt = null;
    private GameObject distractionContainerGO;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    private GameObject GetDistractionObject(DistractionOption options)
    {
        switch (options)
        {
            case DistractionOption.BlindingLight:
                return blindingLight;
            case DistractionOption.InsectSwarm:
                return insectSwarm;
            case DistractionOption.NoiseToGoto:
                return noiseToGoTo;
            case DistractionOption.NoiseToLookAt:
                return noiseToLookAt;
            case DistractionOption.SightToGoTo:
                return sightToGoTo;
            case DistractionOption.SightToLookAt:
                return sightToLookAt;
            default:
                return null;
        }
    }

    public void SpawnAtPosition(Vector3 position, DistractionOption option)
    {
        GameObject go = GetDistractionObject(option);
        if (go == null || position == null)
            return;

        Distraction preD = go.GetComponent<Distraction>();
        if (preD == null)
        {
            Debug.LogWarning("Distraction component missing");
            return;
        }

        Instantiate(go, position, Quaternion.identity, transform);
        go.GetComponent<Distraction>().option = option;
    }
}
