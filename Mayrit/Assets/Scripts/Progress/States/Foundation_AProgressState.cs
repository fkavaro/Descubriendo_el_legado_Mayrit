using System;
using UnityEngine;

public class Foundation_AProgressState : AProgressState
{
    public Foundation_AProgressState(ProgressManager.MilestoneEntry milestone, FiniteStateMachine<ProgressManager> stateMachine)
    : base(milestone, "Foundation", stateMachine) { }

    public override void StartState()
    {

    }

    public override void UpdateState()
    {

    }
}
