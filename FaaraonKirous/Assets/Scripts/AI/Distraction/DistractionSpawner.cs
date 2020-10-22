using System;
using UnityEngine;
using UnityEngine.Assertions;

public class DistractionSpawner : MonoBehaviour
{
    public static string DISTRACTION_CONTAINER = "DistractionContainer";
    [SerializeField] private GameObject blindingLight = null;
    [SerializeField] private GameObject insectSwarm = null;
    [SerializeField] private GameObject inspectableNoise = null;
    [SerializeField] private GameObject somethingToGoTo = null;
    [SerializeField] private GameObject somethingToLookAt = null;
    private GameObject distractionContainerGO;

    private void Awake()
    {
        distractionContainerGO = GameObject.Find(DISTRACTION_CONTAINER);
        if (distractionContainerGO == null)
        {
            distractionContainerGO = Instantiate(new GameObject(), Vector3.zero, Quaternion.identity);
            distractionContainerGO.name = DISTRACTION_CONTAINER;
        }
        Assert.IsNotNull(distractionContainerGO, "WTF?");
    }

    private GameObject GetDistractionObject(DistractionOption options)
    {
        switch (options)
        {
            case DistractionOption.BlindingLight:
                return blindingLight;
            case DistractionOption.InsectSwarm:
                return insectSwarm;
            case DistractionOption.InspectableNoise:
                return inspectableNoise;
            case DistractionOption.SomethingToGoTo:
                return somethingToGoTo;
            case DistractionOption.SomethingToLookAt:
                return somethingToLookAt;
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

        Instantiate(go, position, Quaternion.identity, distractionContainerGO.transform);
        go.GetComponent<Distraction>().option = option;
    }
}
