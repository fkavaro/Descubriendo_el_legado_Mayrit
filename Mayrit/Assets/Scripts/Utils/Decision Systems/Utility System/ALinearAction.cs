
/// <summary>
/// Base class for actions that have a linear decision factor (float).
/// </summary>
public abstract class ALinearAction<TController> : AAction<TController, float> where TController : ABehaviourController<TController>
{
    bool _inverted;

    protected ALinearAction(string name, UtilitySystem<TController> utilitySystem, bool inverted = false)
    : base(name, utilitySystem)
    {
        _inverted = inverted;
    }

    protected override float CalculateUtility()
    {
        utility = _decisionFactor; // Linear function

        if (_inverted)
            utility = 1f - utility; // Inverted linear function

        //Debug.Log(name + " utility: " + utility);
        return utility;
    }
}
