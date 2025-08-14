using UnityEngine;

public class MaslamaSchool_AProgressState : AProgressState
{
    public MaslamaSchool_AProgressState(ProgressManager.Milestone milestone,
    InformationSO milestoneInfoSO,
    FiniteStateMachine<ProgressManager> stateMachine)
    : base("Mathematics and Astronomy school", milestone, milestoneInfoSO, stateMachine) { }

    public override void StartState()
    {
        ProgressManager.Instance.InvokeOnTimeSet(10f);
    }

    public override void UpdateState()
    {

    }
}