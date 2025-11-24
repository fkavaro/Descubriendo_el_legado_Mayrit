using System;
using UnityEngine;

public class Foundation_AProgressState : AProgressState
{
    public Foundation_AProgressState(ProgressManager.Milestone milestone,
    Milestone_InformationSO milestoneInfoSO)
    : base("Foundation", milestone, milestoneInfoSO) { }
}
