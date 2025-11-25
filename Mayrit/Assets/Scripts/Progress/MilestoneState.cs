using System;
using UnityEngine;

public class MilestoneState : AState
{
    readonly MilestoneMapping _milestoneMapping;
    public MilestoneMapping MilestoneMapping => _milestoneMapping;

    public MilestoneState(MilestoneMapping milestoneMapping)
    : base(milestoneMapping.Data.Header)
    {
        _milestoneMapping = milestoneMapping;
    }
}