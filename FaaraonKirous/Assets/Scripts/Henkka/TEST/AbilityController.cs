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
            abilityOption = AbilityOption.PosessAI;
        else if (Input.GetKeyDown(KeyCode.Alpha8))
            abilityOption = AbilityOption.TestSight;

        if (Input.GetButtonDown("Fire2"))
        {
            abilityLayerMask = abilityOption == AbilityOption.PosessAI ? RayCaster.clickSelectorLayerMask : RayCaster.clickSpawnerLayerMask;

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

        if (abilityOption != AbilityOption.PosessAI)
            DeselectAI();

        if (abilityOption == AbilityOption.PosessAI)
        {
            if (selectedAI)
            {
                //In case timer runs out before reacting
                if (selectedAI.isPosessed)
                {
                    SpawnAutoRemoved(hit.point, abilityOption);
                    selectedAI.ControlAI(hit.point);
                }

                DeselectAI();
            }
            else if (hit.collider.CompareTag(RayCaster.CLICK_SELECTOR_TAG))
            {
                SelectAI(hit.collider.gameObject.GetComponentInParent<Character>());
                if (selectedAI)
                    selectedAI.PosessAI();
            }
        }
        else if (abilityOption == AbilityOption.TestSight)
        {
            SpawnRemovable(hit.point, abilityOption);
            //SpawnAutoRemoved(hit.point, abilityOption);
            Debug.Log("Test Sight");
        }
        else if (abilityOption < AbilityOption.NoMoreDistractions)
        {
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

    private void SpawnAutoRemoved(Vector3 pos, AbilityOption option)
    {
        abilitySpawner.SpawnAtPosition(pos, option);
    }

    private void SpawnRemovable(Vector3 pos, AbilityOption option)
    {
        lastSpawnedAbility = abilitySpawner.SpawnAtPosition(pos, option);
    }
}
