using System;
using UnityEngine;

public class Vision_AProgressState : AProgressState
{
    public Vision_AProgressState(ProgressManager.Milestone milestone,
    Milestone_InformationSO milestoneInfoSO)
    : base("Vision", milestone, milestoneInfoSO) { }
}