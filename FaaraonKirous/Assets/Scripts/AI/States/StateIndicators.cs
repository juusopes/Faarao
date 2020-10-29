using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/StateIndicator", order = 1)]
public class StateIndicators : ScriptableObject
{
    public Sprite PatrolStateSprite;
    public Sprite AlertStateSprite;
    public Sprite ChaseStateSprite;
    public Sprite TrackingStateSprite;
    public Sprite DistractedStateSprite;
    public Sprite ControlledStateSprite;
    public Sprite DetectionState;

    public Sprite GetIndicator(StateOption stateOption)
    {
        if (stateOption == StateOption.PatrolState)
            return PatrolStateSprite;
        else if (stateOption == StateOption.AlertState)
            return AlertStateSprite;
        else if (stateOption == StateOption.ChaseState)
            return ChaseStateSprite;
        else if (stateOption == StateOption.TrackingState)
            return TrackingStateSprite;
        else if (stateOption == StateOption.DistractedState)
            return DistractedStateSprite;
        else if (stateOption == StateOption.ControlledState)
            return ControlledStateSprite;
        else if (stateOption == StateOption.DetectionState)
            return DetectionState;

        return null;
    }
}
