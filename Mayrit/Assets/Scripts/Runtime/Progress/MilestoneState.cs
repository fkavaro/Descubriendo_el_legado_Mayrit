using System;
using UnityEngine;

public class MilestoneState : AState
{
    public Milestone_DataSO Data => _milestoneData;
    readonly Milestone_DataSO _milestoneData;

    public MilestoneState(Milestone_DataSO milestoneData)
    : base(milestoneData.Header)
    {
        _milestoneData = milestoneData;
    }
}