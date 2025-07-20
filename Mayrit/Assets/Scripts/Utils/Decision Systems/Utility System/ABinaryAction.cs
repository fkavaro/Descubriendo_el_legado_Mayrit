
/// <summary>
/// Base class for actions that have a binary decision factor (true/false).
/// </summary>
public abstract class ABinaryAction<TController> : AAction<TController, bool> where TController : ABehaviourController<TController>
{
    bool _inverted;
    float _maxValue = 1f;

    protected ABinaryAction(string name, UtilitySystem<TController> utilitySystem, bool inverted = false)
    : base(name, utilitySystem)
    {
        _inverted = inverted;
    }

    protected ABinaryAction(string name, UtilitySystem<TController> utilitySystem, float maxValue, bool inverted = false)
    : base(name, utilitySystem)
    {
        _inverted = inverted;
        _maxValue = maxValue;
    }

    protected override float CalculateUtility()
    {
        if (_inverted)
        {
            if (_decisionFactor)
                utility = 0f;
            else
                utility = _maxValue;
        }
        else
        {
            if (_decisionFactor)
                utility = _maxValue;
            else
                utility = 0f;
        }

        //Debug.Log(name + " utility: " + utility);
        return utility;
    }
}
