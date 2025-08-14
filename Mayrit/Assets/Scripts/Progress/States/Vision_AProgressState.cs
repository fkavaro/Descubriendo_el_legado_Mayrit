using System;
using UnityEngine;

public class Vision_AProgressState : AProgressState
{
    public Vision_AProgressState(ProgressManager.Milestone milestone,
    InformationSO milestoneInfoSO,
    FiniteStateMachine<ProgressManager> stateMachine)
    : base("Vision", milestone, milestoneInfoSO, stateMachine) { }

    public override void StartState()
    {
        ProgressManager.Instance.InvokeOnTimeSet(17f);
    }

    public override void UpdateState()
    {

    }
}