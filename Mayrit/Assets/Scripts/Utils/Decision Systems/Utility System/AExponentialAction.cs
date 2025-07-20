using System;

/// <summary>
/// Base class for actions that have an exponential decision factor (float).
/// </summary>
public abstract class AExponentialAction<TController> : AAction<TController, float> where TController : ABehaviourController<TController>
{
    bool _inverted;

    protected AExponentialAction(string name, UtilitySystem<TController> utilitySystem, bool inverted = false)
    : base(name, utilitySystem)
    {
        _inverted = inverted;
    }

    protected override float CalculateUtility()
    {
        utility = (float)Math.Pow(_decisionFactor, 2); // Exponential function

        if (_inverted)
            utility = 1f - utility; // Inverted exponential function

        //Debug.Log(name + " utility: " + utility);
        return utility;
    }
}