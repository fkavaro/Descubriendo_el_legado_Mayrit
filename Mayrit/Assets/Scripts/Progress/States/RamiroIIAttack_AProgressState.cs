using UnityEngine;

public class RamiroIIAttack_AProgressState : AProgressState
{
    public RamiroIIAttack_AProgressState(ProgressManager.Milestone milestone,
    InformationSO milestoneInfoSO,
    FiniteStateMachine<ProgressManager> stateMachine)
    : base("Ramiro II attack", milestone, milestoneInfoSO, stateMachine) { }

    public override void StartState()
    {
        ProgressManager.Instance.InvokeOnTimeSet(18f);
    }

    public override void UpdateState()
    {

    }
}
