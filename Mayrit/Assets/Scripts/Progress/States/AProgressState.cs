using System;
using UnityEngine;

public abstract class AProgressState : AState<ProgressManager, FiniteStateMachine<ProgressManager>>
{
    public readonly ProgressManager.MilestoneEntry _milestone;

    public AProgressState(ProgressManager.MilestoneEntry milestone, string name, FiniteStateMachine<ProgressManager> stateMachine)
    : base(name, stateMachine)
    {
        _milestone = milestone;
    }

    public override void SwitchState(AState<ProgressManager, FiniteStateMachine<ProgressManager>> nextState)
    {
        _stateMachine?.SwitchState(nextState);

        ProgressManager.Instance._currentMilestone = _milestone;
        ProgressManager.Instance.InvokeOnMilestoneChanged();
    }
}