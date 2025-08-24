using System;
using UnityEngine;

/// <summary>
/// Base class for actions that have a logarithmic decision factor (float).
/// </summary>
public abstract class ALogarithmicAction : AAction<float>
{
    bool _inverted;

    protected ALogarithmicAction(string name, UtilitySystem utilitySystem, bool inverted = false)
    : base(name, utilitySystem)
    {
        _inverted = inverted;
    }

    protected override float CalculateUtility()
    {
        utility = (float)Math.Log(DecisionFactor + 1); // Logarithmic function

        if (_inverted)
            utility = 1f - utility; // Inverted logarithmic function

        //Debug.Log(name + " utility: " + utility);
        return utility;
    }
}