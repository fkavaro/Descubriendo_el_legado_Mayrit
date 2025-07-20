
/// <summary>
/// Base class for actions in the Utility System.
/// </summary>
public abstract class AAction<TController, TFactor> : IAction where TController : ABehaviourController<TController>
{
    public string Name => name;
    public float Utility => CalculateUtility();

    protected string name;
    protected float utility;
    protected TController _controller;
    protected UtilitySystem<TController> _utilitySystem;
    protected TFactor _decisionFactor => SetDecisionFactor();

    public AAction(string name, UtilitySystem<TController> utilitySystem)
    {
        this.name = name;
        _controller = utilitySystem.controller;
        _utilitySystem = utilitySystem;
        utilitySystem.AddAction(this);
    }

    protected abstract TFactor SetDecisionFactor();
    protected abstract float CalculateUtility();
    public abstract void StartAction();
    public abstract void UpdateAction();
    public abstract bool IsFinished();
    public virtual void Reset() { }

    /// <returns>Action name</returns>
    public virtual string DebugDecision()
    {
        return Name;
    }
}
