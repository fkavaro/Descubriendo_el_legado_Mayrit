using System;
using UnityEngine;

public abstract class AProgressState : AState
{
    public readonly ProgressManager.Milestone _milestone;
    public readonly Milestone_InformationSO _informationSO;

    public AProgressState(string name,
        ProgressManager.Milestone milestone,
        Milestone_InformationSO milestoneInfoSo)
    : base(name)
    {
        _milestone = milestone;
        _informationSO = milestoneInfoSo;
    }
}