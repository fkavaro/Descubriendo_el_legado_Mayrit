using System;
using System.Collections.Generic;
using System.Linq;
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

    [Header("Milestone properties")]
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

        // Notify listeners about the initial milestone
        OnMilestoneChanged?.Invoke(_currentMilestone.milestone);

        // Set current playable character
        GameManager.Instance.GetCurrentPlayableCharacter();
    }

    protected override void OnUpdate()
    {

    }

    protected override ADecisionSystem<ProgressManager> CreateDecisionSystem()
    {
        _fsm = new(this);

        _foundationState = new(_milestones[0], _fsm);
        _conquestState = new(_milestones[^1], _fsm); // Last in list

        _fsm.SetInitialState(_conquestState);

        return _fsm;
    }
    #endregion

    #region PUBLIC METHODS
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

        // Update current playable character
        GameManager.Instance.GetCurrentPlayableCharacter();
    }

    public bool AtLastMilestone()
    {
        return _currentMilestone.Equals(_milestones[^1]);
    }

    public bool AtFirstMilestone()
    {
        return _currentMilestone.Equals(_milestones[0]);
    }
    #endregion
}
