using System;
using UnityEngine;

public abstract class AProgressState : AState<FiniteStateMachine>
{
    public readonly Milestone_InformationSO _informationSO;
    public readonly ProgressManager.Milestone _milestone;

    public AProgressState(string name,
        ProgressManager.Milestone milestone,
    Milestone_InformationSO milestoneInfoSo,
    FiniteStateMachine stateMachine)
    : base(name, stateMachine)
    {
        _milestone = milestone;
        _informationSO = milestoneInfoSo;
    }

    public override void AwakeState()
    {
        ProgressManager.Instance._currentMilestone = _milestone;
        ProgressManager.Instance.InvokeOnMilestoneChanged();
        ProgressManager.Instance.InvokeOnTimeSet(_informationSO.WantedTime);
    }
}