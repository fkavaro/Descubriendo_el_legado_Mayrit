using System;
using UnityEngine;

public class Foundation_AProgressState : AProgressState
{
    public Foundation_AProgressState(ProgressManager.Milestone milestone,
    InformationSO milestoneInfoSO,
    FiniteStateMachine<ProgressManager> stateMachine)
    : base("Foundation", milestone, milestoneInfoSO, stateMachine) { }

    public override void StartState()
    {
        ProgressManager.Instance.InvokeOnTimeSet(8f);
    }

    public override void UpdateState()
    {

    }
}
