using System;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Assertions;

public class SomeClass : MonoBehaviour
{

}

public class AbilitySpawner : MonoBehaviour
{
    private static AbilitySpawner _instance;
    public static AbilitySpawner Instance { get { return _instance; } }

    [SerializeField] private GameObject blindingLight = null;
    [SerializeField] private GameObject insectSwarm = null;
    [SerializeField] private GameObject noiseToGoTo = null;
    [SerializeField] private GameObject noiseToLookAt = null;
    [SerializeField] private GameObject sightToGoTo = null;
    [SerializeField] private GameObject sightToLookAt = null;
    [SerializeField] private GameObject testSight = null;
    [SerializeField] private GameObject possessMarker = null;
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

    private GameObject GetObject(AbilityOption options)
    {
        switch (options)
        {
            case AbilityOption.DistractBlindingLight:
                return blindingLight;
            case AbilityOption.DistractInsectSwarm:
                return insectSwarm;
            case AbilityOption.DistractNoiseToGoto:
                return noiseToGoTo;
            case AbilityOption.DistractNoiseToLookAt:
                return noiseToLookAt;
            case AbilityOption.DistractSightToGoTo:
                return sightToGoTo;
            case AbilityOption.DistractSightToLookAt:
                return sightToLookAt;
            case AbilityOption.PossessAI:
                return possessMarker;
            case AbilityOption.TestSight:
                return testSight;
            default:
                return null;
        }
    }

    public GameObject SpawnAtPosition(Vector3 position, AbilityOption option)
    {
        GameObject go = GetObject(option);
        if (go == null)
            return null;

        GameObject newGo = Instantiate(go, position, Quaternion.identity, transform);

        Distraction D = newGo.GetComponent<Distraction>();
        if (D != null)
            D.option = option;

        return newGo;
    }
}
