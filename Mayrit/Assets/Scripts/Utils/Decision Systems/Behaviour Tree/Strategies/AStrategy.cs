using UnityEngine;

/// <summary>
/// AStrategy defines the contract for all strategies used in the behaviour tree nodes.
/// </summary>
public abstract class AStrategy
{
    protected readonly ANPC _npc;
    protected readonly IBehaviourControllable _controllable;
    protected bool DebugMode => _controllable.DebugMode;

    public AStrategy(ANPC controller)
    {
        _npc = controller;
        _controllable = controller._controllable;
    }

    public abstract Node.Status Update();
    public virtual void Reset() { }
}
