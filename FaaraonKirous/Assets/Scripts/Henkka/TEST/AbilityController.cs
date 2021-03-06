﻿using UnityEngine;
using UnityEngine.Assertions;

public class AbilityController : MonoBehaviour
{
    private AbilityOption abilityOption;
    public AbilitySpawner abilitySpawner;
    private Character selectedAI;
    private GameObject lastSpawnedAbility;
    private LayerMask abilityLayerMask;

    private LevelController levelCtrl;
    private int click = 0;
    private bool abilityActivated;

    private PlayerController currentPlayerController;

    public SoundManager soundFX;

    private void Start()
    {
        Assert.IsNotNull(abilitySpawner, "Add ability spawner prefab!");

        levelCtrl = GameObject.FindGameObjectWithTag("LevelController").GetComponent<LevelController>();
    }

    private void Update()
    {
        InputAbilities();
    }

    private void InputAbilities()
    {
        if (abilitySpawner == null)
            return;

        if (levelCtrl.currentCharacter == null)
            return;

        currentPlayerController = levelCtrl.currentCharacter.GetComponent<PlayerController>();

        if (!currentPlayerController.abilityActive)
            return;
        //Debug.Log("Overrided pos: " + currentPlayerController.abilityHitPos);
        if (Input.GetKeyDown(KeyCode.Mouse0)
            && currentPlayerController.abilityLimits[currentPlayerController.abilityNum] > 0
            && currentPlayerController.abilityCooldowns[currentPlayerController.abilityNum] == 0)
        {
            //Debug.Log("Activated");
            abilityActivated = true;
            //Debug.Log("InRange: " + CurrentPlayerController.inRange + ", Ability Activated: " + abilityActivated
            //     + ", abilityClicked: " + CurrentPlayerController.abilityClicked + ", SearchingForSight: " + CurrentPlayerController.searchingForSight);
        }
        //Debug.Log(levelCtrl.activeCharacter.GetComponent<PlayerController>().inRange);

        if (currentPlayerController.inRange
            && abilityActivated
            && currentPlayerController.abilityClicked
            && !currentPlayerController.searchingForSight
            && (currentPlayerController.abilityLimits[currentPlayerController.abilityNum] > 0 || (currentPlayerController.playerOne && levelCtrl.currentCharacter.GetComponent<PharaohAbilities>().abilityLimitList[currentPlayerController.abilityNum] == 0) || (!currentPlayerController.playerOne && levelCtrl.currentCharacter.GetComponent<PriestAbilities>().abilityLimitList[currentPlayerController.abilityNum] == 0))
            && currentPlayerController.abilityCooldowns[currentPlayerController.abilityNum] == 0)
        {
            //Debug.Log("Selecting");
            PlayerController caster = currentPlayerController;
            if (caster.abilityNum == 2 && !caster.playerOne)
                abilityOption = AbilityOption.DistractBlindingLight;
            else if (caster.abilityNum == 2 && caster.playerOne)
                abilityOption = AbilityOption.DistractInsectSwarm;
            else if (caster.abilityNum == 3 && !caster.playerOne)
                abilityOption = AbilityOption.DistractNoiseToGoto;
            else if (caster.abilityNum == 3 && caster.playerOne)
                abilityOption = AbilityOption.DistractNoiseToLookAt;
            else if (caster.abilityNum == 4 && caster.playerOne)
                abilityOption = AbilityOption.DistractSightToGoTo;
            else if (caster.abilityNum == 4 && !caster.playerOne)
                abilityOption = AbilityOption.DistractSightToLookAt;
            else if (caster.abilityNum == 5 && caster.playerOne)
                abilityOption = AbilityOption.PossessAI;
            else if (caster.abilityNum == 5 && !caster.playerOne)
                abilityOption = AbilityOption.TestSight;
            else if (caster.abilityNum == 6 && !caster.playerOne)
                abilityOption = AbilityOption.ViewPath;
            else
                abilityOption = AbilityOption.NoMoreDistractions;

            if (abilityOption != AbilityOption.NoMoreDistractions)
            {
                abilityLayerMask = abilityOption == AbilityOption.PossessAI ? RayCaster.clickSelectorLayerMask : RayCaster.clickSpawnerLayerMask;

                RaycastHit hit = RayCaster.ScreenPoint(Input.mousePosition, abilityLayerMask);
                //Debug.Log("hit " + hit.point);

                if (caster.abilityNum == 5)
                {
                    if (RayCaster.HitObject(hit))
                        UseAbility(hit, hit.point);
                }
                else
                {
                    UseAbility(hit, currentPlayerController.abilityHitPos);
                }
            }

            //Ability Ender
            if (caster.abilityNum != 5 && caster.abilityNum != 1 || (caster.abilityNum == 5 && click == 1) || (caster.abilityNum == 1 && caster.playerOne && caster.gameObject.GetComponent<PharaohAbilities>().useInvisibility) || (!caster.playerOne && caster.abilityNum == 1 && caster.gameObject.GetComponent<PriestAbilities>().warped))
            {
                if (!caster.playerOne || (caster.playerOne && caster.abilityNum != 1))
                {
                    caster.IsInvisible = false;
                }
                Debug.Log("Disabled");
                caster.abilityLimitUsed = caster.abilityNum;
                
                if (NetworkManager._instance.ShouldSendToClient)
                {
                    ServerSend.AbilityUsed(currentPlayerController.manager.Type, caster.abilityLimitUsed);
                }
                else if (NetworkManager._instance.ShouldSendToServer)
                {
                    ClientSend.AbilityLimitUsed(currentPlayerController.manager.Type, caster.abilityLimitUsed);
                }

                caster.abilityNum = 0;
                caster.abilityActive = false;
                caster.inRange = false;
                caster.searchingForSight = true;
                caster.abilityClicked = false;
                abilityActivated = false;
                caster.Stay();
                click = 0;
            }
            else
            {
                click++;
                if (caster.abilityNum != 1)
                {
                    abilityActivated = false;
                }
            }
        }
        else
        {
            //abilityActivated = false;
            PlayerController caster = currentPlayerController;
            //caster.abilityClicked = false;
            return;
        }
    }

    private void UseAbility(RaycastHit hit, Vector3 pos)
    {
        if (abilityOption != AbilityOption.PossessAI)
        {
            SoundManager.Instance.FlashBangSound();
        }

        //TODO: Lazy ? no : object pooling...
        //Debug.Log(abilityOption);
        if (lastSpawnedAbility != null)
            Destroy(lastSpawnedAbility);

        if (abilityOption == AbilityOption.PossessAI)
        {
            if (selectedAI)
            {
                SoundManager.Instance.PossessSound();
                PossessEnemy(pos);
                DeselectAI();
            }
            else if (hit.collider && hit.collider.CompareTag(RayCaster.CLICK_SELECTOR_TAG))
            {
                Debug.Log("Select AI");
                SelectAI(hit.collider.gameObject.GetComponentInParent<Character>());
            }
        }
        if (abilityOption == AbilityOption.ViewPath)
        {
            if (selectedAI)
            {
                DeselectAI();
            }
            else if (hit.collider && hit.collider.CompareTag(RayCaster.CLICK_SELECTOR_TAG))
            {
                SelectAI(hit.collider.gameObject.GetComponentInParent<Character>());
            }
        }
        else if (abilityOption == AbilityOption.TestSight)
        {
            DeselectAI();
            SpawnRemovable(pos, abilityOption);
        }
        else if (abilityOption < AbilityOption.NoMoreDistractions)
        {
            DeselectAI();
            SpawnAutoRemoved(pos, abilityOption);
        }
    }

    private void SelectAI(Character character)
    {
        selectedAI = character;
        //if (selectedAI != null)
        //    selectedAI.selectionIndicator.SetActive(true);
    }


    private void DeselectAI()
    {
        // if (selectedAI != null)
        //   selectedAI.selectionIndicator.SetActive(false);

        selectedAI = null;
    }

    private void PossessEnemy(Vector3 destinationPoint)
    {
        //Debug.Log("TRYING Posess AI" + destinationPoint);
        //if (!OnNavMesh.IsPartiallyReachable(selectedAI.transform, destinationPoint))
        //{
        //     destinationPoint = OnNavMesh.GetClosestPointOnNavmesh(destinationPoint);
        //}
        //Debug.Log("Posess AI");
        // if (!OnNavMesh.IsPartiallyReachable(selectedAI.transform, destinationPoint))
        // {
        SpawnAutoRemoved(destinationPoint, abilityOption);
        if (NetworkManager._instance.IsHost)
            selectedAI.PossessAI(destinationPoint);
        else if (NetworkManager._instance.ShouldSendToServer)
            ClientSend.EnemyPossessed(selectedAI.Id, destinationPoint);
        // }
        //}
        //else
        //{
        //TODO: Spawn failed marker
        //}
    }

    private void SpawnAutoRemoved(Vector3 pos, AbilityOption option)
    {
        if (option < AbilityOption.NoMoreDistractions || option == AbilityOption.PossessAI)
        {
            if (NetworkManager._instance.ShouldSendToServer)
            {
                ClientSend.AbilityUsed(option, pos);
            }
            else if (NetworkManager._instance.ShouldSendToClient)
            {
                ServerSend.AbilityVisualEffectCreated(option, pos);
            }
        }
        abilitySpawner.SpawnAtPosition(pos, option);
    }

    private void SpawnRemovable(Vector3 pos, AbilityOption option)
    {
        lastSpawnedAbility = abilitySpawner.SpawnAtPosition(pos, option);
    }
}
