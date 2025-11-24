using System;
using UnityEngine;

public class Albacar_AProgressState : AProgressState
{
    public Albacar_AProgressState(ProgressManager.Milestone milestone,
    Milestone_InformationSO milestoneInfoSO)
    : base("Albacar", milestone, milestoneInfoSO) { }
}
