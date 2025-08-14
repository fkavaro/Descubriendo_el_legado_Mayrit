using System;
using UnityEngine;

public class Conquest_AProgressState : AProgressState
{
    public Conquest_AProgressState(ProgressManager.Milestone milestone,
    InformationSO milestoneInfoSO,
    FiniteStateMachine<ProgressManager> stateMachine)
    : base("Conquest", milestone, milestoneInfoSO, stateMachine) { }

    public override void StartState()
    {
        ProgressManager.Instance.InvokeOnTimeSet(15f);
    }

    public override void UpdateState()
    {

    }
}
