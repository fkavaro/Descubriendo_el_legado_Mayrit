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

    // public override void StartState()
    // {
    //     Debug.Log($"Trying to start milestone state: {_milestoneData.Header}");

    //     // Load milestone scene
    //     ServiceLocator.Instance.Get<ScenesController>().NewTransitionPlan()
    //         .Load(SceneDatabase.Slot.Milestone, _milestoneData.SceneName, setActive: true)
    //         .ClearAssets()
    //         .Perform();

    //     base.StartState();
    // }
}