
using UnityEngine;

/// <summary>
/// Base class for actions that have a binary decision factor (true/false).
/// </summary>
public abstract class ABinaryAction : AAction<bool>
{
    bool _inverted;
    float _maxValue = 1f;

    protected ABinaryAction(string name, UtilitySystem utilitySystem, bool inverted = false)
    : base(name, utilitySystem)
    {
        _inverted = inverted;
    }

    protected ABinaryAction(string name, UtilitySystem utilitySystem, float maxValue, bool inverted = false)
    : base(name, utilitySystem)
    {
        _inverted = inverted;
        _maxValue = maxValue;
    }

    protected override float CalculateUtility()
    {
        if (_inverted)
        {
            if (DecisionFactor)
                utility = 0f;
            else
                utility = _maxValue;
        }
        else
        {
            if (DecisionFactor)
                utility = _maxValue;
            else
                utility = 0f;
        }

        //Debug.Log(name + " utility: " + utility);
        return utility;
    }
}
