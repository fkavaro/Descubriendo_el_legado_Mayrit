
/// <summary>
/// Interface for objects with behaviour.
/// </summary>
public interface IBehaviourControllable
{
    string Name { get; }
    ADecisionSystem DecisionSystem { get; }

    /// <summary>
    /// Sets the decision system for this controllable.
    /// Will be executed in ADecisionSystem Awake.
    /// </summary>
    void SetDecisionSystem();
}
