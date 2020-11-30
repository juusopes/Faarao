using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Detector
{
    #region Fields
    Character character;
    private DetectionPercentageCalculator detectionPercentageCalculator;
    private Transform distractionContainer;
    public float detectionPercentage;
    #endregion

    #region Expressions
    private GameObject Player1 => character.Player1;
    private GameObject Player2 => character.Player2;
    private PlayerController Player1Controller => character.Player1Controller;
    private PlayerController Player2Controller => character.Player2Controller;
    private float HearingRange => character.HearingRange;
    private float MaxSightRange => character.MaxSightRange;
    private float SightRange => character.SightRange;
    private float SightSpeed => character.SightSpeed;
    private float SightRangeCrouching => character.SightRangeCrouching;
    private float FOV => character.FOV;
    private Transform transform => character.gameObject.transform;

    #endregion

    #region Start and run

    public Detector(Character character)
    {
        this.character = character;
        detectionPercentageCalculator = new DetectionPercentageCalculator(SightSpeed, SightRange);

        if (AbilitySpawner.Instance)
            distractionContainer = AbilitySpawner.Instance.transform;
        if (!distractionContainer)
            Debug.LogWarning("Did not find DistractionSpawner");
    }

    public void RunDetection()
    {
        DetectDistractions();
        TryDetectPlayers();
    }

    #endregion


    #region Call Detections

    private void TryDetectPlayers()
    {
        detectionPercentage = detectionPercentageCalculator.SimulateFOVPercentage(character.CouldDetectAnyPlayer);

        character.couldDetectPlayer1 = CouldDetectPlayer(Player1, Player1Controller);
        character.couldDetectPlayer2 = CouldDetectPlayer(Player2, Player2Controller);

        bool sim1Reached = SimulationReached(Player1, detectionPercentage);
        bool sim2Reached = SimulationReached(Player2, detectionPercentage);
        bool hasReached = sim1Reached | sim2Reached;

        if (sim1Reached)
        {
            character.KillPlayer(Player1);
        }
        if (sim2Reached)
        {
            character.KillPlayer(Player2);
        }

        SightVisualsCalculated(detectionPercentage, hasReached);
    }

    public void SightVisualsCalculated(float percentage, bool hasReached)
    {
        LineType lt;
        if (hasReached)
            lt = LineType.Red;
        else if (character.CouldDetectAnyPlayer)
            lt = LineType.Yellow;
        else
            lt = LineType.Green;

        character.UpdateSightVisuals(percentage, lt);
    }

    private bool SimulationReached(GameObject testObj, float percentage)
    {
        float objDistance = Vector3.Distance(testObj.transform.position, character.transform.position);
        float fovDistance = MaxSightRange * percentage;
        //Debug.Log("Distance : " + objDistance + "fovDistance : " + fovDistance);
        return objDistance < fovDistance;
    }

    private void DetectDistractions()
    {
        //TODO: fix raycast position
        if (distractionContainer == null)
            return;

        bool distractionAssigned = false;
        bool testerAssigned = false;

        for (int i = distractionContainer.childCount - 1; i >= 0; i--)
        {
            Transform childTransform = distractionContainer.GetChild(i);
            if (character.currentDistraction != null)
                if (childTransform == character.currentDistraction.transform)
                    return;

            Distraction distraction = childTransform.GetComponent<Distraction>();
            if (distraction)
            {
                if (IsDistractionDetectable(distraction))
                {
                    if (distraction.option == AbilityOption.TestSight)
                    {
                        if (!testerAssigned)
                        {
                            testerAssigned = true;
                            character.testSightDetection.DisplaySightTester(true, distraction.transform.position + Vector3.up, LineType.White);
                        }
                    }
                    else
                    {
                        if (!distractionAssigned)
                        {
                            distractionAssigned = true;
                            character.ReceiveDistraction(distraction);
                        }

                    }
                }
            }
        }

        if (!testerAssigned && !distractionAssigned)
            character.testSightDetection.DisplaySightTester(false, Vector3.zero, LineType.White);
    }

    private bool IsDistractionDetectable(Distraction distraction)
    {
        if (distraction.detectionType == DetectionType.hearing)
            if (ObjectIsInHearingRange(distraction.gameObject))
                return true;

        if (distraction.detectionType == DetectionType.sight)
            if (CouldDetectDistraction(distraction.gameObject))
            {
                return true;
            }

        return false;
    }

    #endregion

    #region Calculate Detection

    /// <summary>
    /// Returns true if player is in sight, fov and can be raycasted. Allows sightline simulation to begin, which dictates actually seeing player.
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    private bool CouldDetectPlayer(GameObject player, PlayerController playerController)
    {
        if (IsPlayerDeadOrInvisible(playerController))   //Call this first so we dont mark targets
            return false;
        if (IsPlayerCrouchingAboveSightLevel(player, playerController))
            return false;
        if (character.isPosessed)
            return false;
        float testRange = playerController != null && playerController.IsCrouching ? SightRangeCrouching : SightRange;
        return TestDetection(player, testRange, RayCaster.playerDetectLayerMask, RayCaster.PLAYER_TAG);
    }

    private bool IsPlayerDeadOrInvisible(PlayerController playerController)
    {
        if (playerController == null)
            return true;
        if (playerController.IsDead || playerController.isInvisible)
            return true;

        return false;
    }

    private bool IsPlayerCrouchingAboveSightLevel(GameObject player, PlayerController playerController)
    {
        if (playerController == null)
            return true;
        if (playerController.IsCrouching && player.transform.position.y > character.fieldOfViewGO.transform.position.y)
            return true;

        return false;
    }

    private bool CouldDetectDistraction(GameObject testObj)
    {
        return TestDetection(testObj, SightRange, RayCaster.distractionLayerMask, RayCaster.DISTRACTION_TAG);
    }

    /// <summary>
    /// Returns true if object is in sight, fov and can be raycasted.
    /// </summary>
    /// <param name="testObj"></param>
    /// <returns></returns>
    private bool TestDetection(GameObject testObj, float range, LayerMask layermask, string tag)
    {
        return ObjectIsInRange(testObj, range) && ObjectIsInFov(testObj) && CanRayCastObject(testObj, layermask, tag);
    }

    private bool ObjectIsInRange(GameObject testObj, float range)
    {
        return Vector3.Distance(testObj.transform.position, transform.position) <= range;
    }

    private bool ObjectIsInFov(GameObject testObj)
    {
        Vector3 dirToObj = (testObj.transform.position - character.fieldOfViewGO.transform.position).normalized;
        if (Vector3.Angle(character.fieldOfViewGO.transform.forward, dirToObj) < FOV / 2f)
        {
            return true;
        }
        return false;
    }
    private bool ObjectIsInHearingRange(GameObject testObj)
    {
        return Vector3.Distance(testObj.transform.position, transform.position) <= HearingRange;
    }

    private bool CanRayCastObject(GameObject testObj, LayerMask layerMask, string tag = "")
    {
        RaycastHit hit = RayCaster.ToTarget(character.gameObject, testObj, SightRange, layerMask);
        if (RayCaster.HitObject(hit, tag))
        {
            if (tag == RayCaster.PLAYER_TAG)
                character.chaseTarget = hit.transform.position;
            return true;
        }

        return false;
    }

    #endregion
}
