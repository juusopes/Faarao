public enum AbilityOption : byte
{
    DistractBlindingLight,
    DistractInsectSwarm,
    DistractNoiseToGoto,
    DistractNoiseToLookAt,
    DistractSightToGoTo,
    DistractSightToLookAt,

    NoMoreDistractions = 9,    //Any value smaller than this is distraction

    PosessAI,
    TestSight,
    ViewPath
}
