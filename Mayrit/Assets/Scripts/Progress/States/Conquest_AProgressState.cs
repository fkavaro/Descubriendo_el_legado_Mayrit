using System;
using UnityEngine;

public class Conquest_AProgressState : AProgressState
{
    public Conquest_AProgressState(ProgressManager.Milestone milestone,
    Milestone_InformationSO milestoneInfoSO)
    : base("Conquest", milestone, milestoneInfoSO) { }
}
