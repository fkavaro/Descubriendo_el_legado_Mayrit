using System;
using UnityEngine;

public class Conquest_AProgressState : AProgressState
{
    public Conquest_AProgressState(ProgressManager.MilestoneEntry milestone, StackFiniteStateMachine<ProgressManager> stateMachine)
    : base(milestone, "Conquest", stateMachine) { }

    public override void UpdateState()
    {

    }
}
