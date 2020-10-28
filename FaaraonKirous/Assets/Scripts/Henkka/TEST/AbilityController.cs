using UnityEngine;
using UnityEngine.Assertions;

public class AbilityController : MonoBehaviour
{
    private AbilityOption abilityOption;
    public AbilitySpawner abilitySpawner;
    private Character selectedAI;
    private GameObject lastSpawnedAbility;
    private LayerMask abilityLayerMask;

    private LevelController levelCtrl;

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

        if (!levelCtrl.activeCharacter.GetComponent<PlayerController>().abilityActive)
            return;

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            PlayerController caster = levelCtrl.activeCharacter.GetComponent<PlayerController>();
            if (caster.abilityNum == 1)
                abilityOption = AbilityOption.DistractBlindingLight;
            else if (caster.abilityNum == 2)
                abilityOption = AbilityOption.DistractInsectSwarm;
            else if (caster.abilityNum == 3)
                abilityOption = AbilityOption.DistractNoiseToGoto;
            else if (caster.abilityNum == 4)
                abilityOption = AbilityOption.DistractNoiseToLookAt;
            else if (caster.abilityNum == 5)
                abilityOption = AbilityOption.DistractSightToGoTo;
            else if (caster.abilityNum == 6)
                abilityOption = AbilityOption.DistractSightToLookAt;
            else if (caster.abilityNum == 7)
                abilityOption = AbilityOption.PossessAI;
            else if (caster.abilityNum == 8)
                abilityOption = AbilityOption.TestSight;
            else if (caster.abilityNum == 9)
                abilityOption = AbilityOption.ViewPath;
            else
                abilityOption = AbilityOption.NoMoreDistractions;

            if (abilityOption != AbilityOption.NoMoreDistractions)
            {
                abilityLayerMask = abilityOption == AbilityOption.PossessAI ? RayCaster.clickSelectorLayerMask : RayCaster.clickSpawnerLayerMask;

                RaycastHit hit = RayCaster.ScreenPoint(Input.mousePosition, abilityLayerMask);
                //Debug.Log("hit " + hit.point);

                if (RayCaster.HitObject(hit))
                {
                    UseAbility(hit);
                }
            }
        }
        else
        {
            return;
        }
    }

    private void UseAbility(RaycastHit hit)
    {
        //TODO: Lazy ? no : object pooling...
        Debug.Log(abilityOption);
        if (lastSpawnedAbility != null)
            Destroy(lastSpawnedAbility);

        if (abilityOption == AbilityOption.PossessAI)
        {
            if (selectedAI)
            {
                PossessEnemy(hit.point);
                DeselectAI();
            }
            else if (hit.collider.CompareTag(RayCaster.CLICK_SELECTOR_TAG))
            {
                SelectAI(hit.collider.gameObject.GetComponentInParent<Character>());
            }
        }
        if (abilityOption == AbilityOption.ViewPath)
        {
            if (selectedAI)
            {
                DeselectAI();
            }
            else if (hit.collider.CompareTag(RayCaster.CLICK_SELECTOR_TAG))
            {
                SelectAI(hit.collider.gameObject.GetComponentInParent<Character>());
            }
        }
        else if (abilityOption == AbilityOption.TestSight)
        {
            DeselectAI();
            SpawnRemovable(hit.point, abilityOption);
        }
        else if (abilityOption < AbilityOption.NoMoreDistractions)
        {
            DeselectAI();
            SpawnAutoRemoved(hit.point, abilityOption);
        }
    }

    private void SelectAI(Character character)
    {
        selectedAI = character;
        if (selectedAI != null)
            selectedAI.selectionIndicator.SetActive(true);
    }


    private void DeselectAI()
    {
        if (selectedAI != null)
            selectedAI.selectionIndicator.SetActive(false);

        selectedAI = null;
    }

    private void PossessEnemy(Vector3 destinationPoint)
    {
        GameObject tester = GameObject.FindGameObjectWithTag("Player"); //TODO: GAME MANAGER REFERENCE
        if (OnNavMesh.IsReachable(tester.transform, destinationPoint))
        {
            SpawnAutoRemoved(destinationPoint, abilityOption);
            if (NetworkManager._instance.IsHost)
                selectedAI.PossessAI(destinationPoint);
            else if (NetworkManager._instance.ShouldSendToServer)
                ClientSend.EnemyPossessed(selectedAI.Id, destinationPoint);
        }
        else
        {
            //TODO: Spawn failed marker
        }
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
