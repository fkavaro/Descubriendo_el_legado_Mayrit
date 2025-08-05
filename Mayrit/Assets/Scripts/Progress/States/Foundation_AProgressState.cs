using System;
using UnityEngine;

public class Foundation_AProgressState : AProgressState
{
    public Foundation_AProgressState(ProgressManager.MilestoneEntry milestone,
    StackFiniteStateMachine<ProgressManager> stateMachine,
    AProgressState nextState = null)
    : base(milestone, "Foundation", stateMachine, nextState) { }

    public override void UpdateState()
    {

    }
}
