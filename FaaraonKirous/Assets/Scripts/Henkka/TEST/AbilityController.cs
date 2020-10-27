using UnityEngine;
using UnityEngine.Assertions;

public class AbilityController : MonoBehaviour
{
    private AbilityOption abilityOption;
    public AbilitySpawner abilitySpawner;
    private Character selectedAI;
    private GameObject lastSpawnedAbility;
    private LayerMask abilityLayerMask;

    private void Start()
    {
        Assert.IsNotNull(abilitySpawner, "Add ability spawner prefab!");
    }

    private void Update()
    {
        InputAbilities();
    }

    private void InputAbilities()
    {
        if (abilitySpawner == null)
            return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
            abilityOption = AbilityOption.DistractBlindingLight;
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            abilityOption = AbilityOption.DistractInsectSwarm;
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            abilityOption = AbilityOption.DistractNoiseToGoto;
        else if (Input.GetKeyDown(KeyCode.Alpha4))
            abilityOption = AbilityOption.DistractNoiseToLookAt;
        else if (Input.GetKeyDown(KeyCode.Alpha5))
            abilityOption = AbilityOption.DistractSightToGoTo;
        else if (Input.GetKeyDown(KeyCode.Alpha6))
            abilityOption = AbilityOption.DistractSightToLookAt;
        else if (Input.GetKeyDown(KeyCode.Alpha7))
            abilityOption = AbilityOption.PossessAI;
        else if (Input.GetKeyDown(KeyCode.Alpha8))
            abilityOption = AbilityOption.TestSight;
        else if (Input.GetKeyDown(KeyCode.Alpha9))
            abilityOption = AbilityOption.ViewPath;
        else
            abilityOption = AbilityOption.NoMoreDistractions;


        if (abilityOption != AbilityOption.NoMoreDistractions)
        {
            abilityLayerMask = abilityOption == AbilityOption.PossessAI ? RayCaster.clickSelectorLayerMask : RayCaster.clickSpawnerLayerMask;

            RaycastHit hit = RayCaster.ScreenPoint(Input.mousePosition, abilityLayerMask);
            Debug.Log("hit " + hit.point);

            if (RayCaster.HitObject(hit))
            {
                UseAbility(hit);
            }

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
