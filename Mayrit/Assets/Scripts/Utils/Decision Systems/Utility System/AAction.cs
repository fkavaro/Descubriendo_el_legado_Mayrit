
using UnityEngine;

/// <summary>
/// Base class for actions in the Utility System.
/// </summary>
public abstract class AAction<TFactor> : IAction
{
    public string Name => name;
    public float Utility => CalculateUtility();

    protected string name;
    protected float utility;
    protected UtilitySystem _utilitySystem;
    protected TFactor DecisionFactor => SetDecisionFactor();

    public AAction(string name, UtilitySystem utilitySystem)
    {
        this.name = name;
        _utilitySystem = utilitySystem;
        utilitySystem.AddAction(this);
    }

    protected abstract TFactor SetDecisionFactor();
    protected abstract float CalculateUtility();
    public abstract void StartAction();
    public abstract void UpdateAction();
    public abstract bool IsFinished();
    public virtual void Reset() { }

    public virtual string DebugDecision()
    {
        return Name;
    }
}
