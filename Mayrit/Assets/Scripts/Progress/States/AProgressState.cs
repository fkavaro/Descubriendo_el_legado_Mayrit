using System;
using UnityEngine;

public abstract class AProgressState : AState<ProgressManager, StackFiniteStateMachine<ProgressManager>>
{
    public readonly ProgressManager.MilestoneEntry _milestone;

    public AProgressState(ProgressManager.MilestoneEntry milestone,
    string name,
    StackFiniteStateMachine<ProgressManager> stateMachine,
    AProgressState nextState = null)
    : base(name, stateMachine, nextState)
    {
        _milestone = milestone;
    }

    public override void StartState()
    {
        ProgressManager.Instance._currentMilestone = _milestone;
        ProgressManager.Instance.InvokeOnMilestoneChanged();

        // Update current playable character
        GameManager.Instance.GetCurrentPlayableCharacter();
    }
}