using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MilestoneState : AState
{
    public Milestone_DataSO Data => _milestoneData;
    readonly Milestone_DataSO _milestoneData;

    public MilestoneState(Milestone_DataSO milestoneData)
    : base(milestoneData.Header)
    {
        _milestoneData = milestoneData;
    }

    public override void StartState()
    {
        // Load milestone scene, if not already loaded
        if (!SceneManager.GetSceneByName(_milestoneData.SceneName.ToString()).isLoaded)
            ServiceLocator.Instance.Get<ScenesController>().NewTransitionPlan()
                .Load(SceneDatabase.SceneType.Milestone, _milestoneData.SceneName, setActive: true)
                .WithOverlay()
                .ClearAssets()
                .Perform();

        base.StartState();
    }
}