using UnityEngine;

public class AlmanzorMeeting_AProgressState : AProgressState
{
    public AlmanzorMeeting_AProgressState(ProgressManager.Milestone milestone,
    Milestone_InformationSO milestoneInfoSO)
    : base("Almanzor meeting", milestone, milestoneInfoSO) { }
}
