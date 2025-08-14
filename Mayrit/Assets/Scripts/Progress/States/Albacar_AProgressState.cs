using System;
using UnityEngine;

public class Albacar_AProgressState : AProgressState
{
    public Albacar_AProgressState(ProgressManager.Milestone milestone,
    InformationSO milestoneInfoSO,
    FiniteStateMachine<ProgressManager> stateMachine)
    : base("Albacar", milestone, milestoneInfoSO, stateMachine) { }

    public override void StartState()
    {
        ProgressManager.Instance.InvokeOnTimeSet(11f);
    }

    public override void UpdateState()
    {

    }
}
