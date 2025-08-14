using UnityEngine;

public class AlmanzorMeeting_AProgressState : AProgressState
{
    public AlmanzorMeeting_AProgressState(ProgressManager.Milestone milestone,
    InformationSO milestoneInfoSO,
    FiniteStateMachine<ProgressManager> stateMachine)
    : base("Almanzor meeting", milestone, milestoneInfoSO, stateMachine) { }

    public override void StartState()
    {
        ProgressManager.Instance.InvokeOnTimeSet(6f);
    }

    public override void UpdateState()
    {

    }
}
