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
    public struct MilestoneEntry
    {
        //public int id;
        public Milestone milestone;
        public MilestoneInformationSO informationSO;
    }


    #region PUBLIC PROPERTIES
    public event Action<Milestone> OnMilestoneChanged;

    public int _currentMilestoneId;
    public MilestoneEntry _currentMilestone;
    public List<MilestoneEntry> _milestones = new();

    // State Machine
    public FiniteStateMachine<ProgressManager> _fsm;
    public Foundation_AProgressState _foundationState;
    public Conquest_AProgressState _conquestState;
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
        _currentMilestoneId = 0;
        _currentMilestone = _milestones[_currentMilestoneId];
    }

    protected override void OnUpdate()
    {

    }

    protected override ADecisionSystem<ProgressManager> CreateDecisionSystem()
    {
        _fsm = new(this);

        //_foundationState = new(_milestones[0], _fsm);
        _conquestState = new(_milestones[^1], _fsm); // Last milestone is the last one in the list

        _fsm.SetInitialState(_conquestState);

        return _fsm;
    }
    #endregion

    #region PUBLIC METHODS
    public void InvokeOnMilestoneChanged()
    {
        OnMilestoneChanged?.Invoke(_currentMilestone.milestone);
    }

    public void SwitchToNextMilestone()
    {
        _currentMilestoneId++;
        ChangeMilestone();
    }

    public void SwitchToPreviousMilestone()
    {
        _currentMilestoneId--;
        ChangeMilestone();
    }
    #endregion

    #region PRIVATE METHODS
    void ChangeMilestone()
    {
        _currentMilestone = _milestones[_currentMilestoneId];
        OnMilestoneChanged?.Invoke(_currentMilestone.milestone);
    }
    #endregion
}
