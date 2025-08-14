using UnityEngine;

public class Almudayna_AProgressState : AProgressState
{
    public Almudayna_AProgressState(ProgressManager.Milestone milestone,
    InformationSO milestoneInfoSO,
    FiniteStateMachine<ProgressManager> stateMachine)
    : base("Almudayna", milestone, milestoneInfoSO, stateMachine) { }

    public override void StartState()
    {
        ProgressManager.Instance.InvokeOnTimeSet(14f);
    }

    public override void UpdateState()
    {

    }
}