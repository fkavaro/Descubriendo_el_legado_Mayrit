using System;
using System.Collections.Generic;
using UnityEngine;

public class ProgressManager : Singleton<ProgressManager>
{
    public enum Milestone
    {
        Foundation,
        Conquest,
    }

    [Serializable]
    public class MilestoneEntry
    {
        public Milestone milestone;
        public MilestoneInformationSO informationSO;
    }


    #region PUBLIC PROPERTIES
    public event Action<Milestone> OnMilestoneChanged;
    public MilestoneEntry _currentMilestone;
    public List<MilestoneEntry> _milestones = new();

    // State Machine
    public FiniteStateMachine<ProgressManager> _fsm;
    public Foundation_AProgressState _foundationState;
    #endregion

    #region PRIVATE PROPERTIES

    #endregion

    #region INHERITED
    protected override void OnAwake()
    {
        // Singleton
        base.OnAwake();
    }

    protected override void OnStart()
    {

    }

    protected override void OnUpdate()
    {

    }
    protected override ADecisionSystem<ProgressManager> CreateDecisionSystem()
    {
        _fsm = new(this);

        _foundationState = new(_milestones[0], _fsm);

        _fsm.SetInitialState(_foundationState);

        return _fsm;
    }


    #endregion

    #region PUBLIC METHODS
    public void InvokeOnMilestoneChanged()
    {
        OnMilestoneChanged?.Invoke(_currentMilestone.milestone);
    }
    #endregion

    #region PRIVATE METHODS

    #endregion
}
