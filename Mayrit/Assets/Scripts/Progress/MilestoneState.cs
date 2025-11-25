using System;
using UnityEngine;

public class MilestoneState : AState
{
    public readonly Milestone_InformationSO _milestoneInformation;

    public MilestoneState(string name, Milestone_InformationSO milestoneInformation)
    : base(name)
    {
        _milestoneInformation = milestoneInformation;
    }
}