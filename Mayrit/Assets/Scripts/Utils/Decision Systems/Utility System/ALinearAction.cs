
using UnityEngine;

/// <summary>
/// Base class for actions that have a linear decision factor (float).
/// </summary>
public abstract class ALinearAction : AAction<float>
{
    bool _inverted;

    protected ALinearAction(string name, UtilitySystem utilitySystem, bool inverted = false)
    : base(name, utilitySystem)
    {
        _inverted = inverted;
    }

    protected override float CalculateUtility()
    {
        utility = DecisionFactor; // Linear function

        if (_inverted)
            utility = 1f - utility; // Inverted linear function

        //Debug.Log(name + " utility: " + utility);
        return utility;
    }
}
