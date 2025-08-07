using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProgressManager : Singleton<ProgressManager>
{
    public enum Milestone
    {
        Vision,
        Foundation,
        Albacar,
        Almudayna,
        RamiroII,
        Almanzor,
        School,
        Conquest,
    }

    [Serializable]
    public struct MilestoneEntry
    {
        public Milestone milestone;
        public MilestoneInformationSO informationSO;
    }


    #region PUBLIC PROPERTIES
    public event Action<Milestone> OnMilestoneChanged;

    [Header("Milestone properties")]
    //public int _currentMilestoneId;
    public MilestoneEntry _currentMilestone;
    public List<MilestoneEntry> _milestones = new();

    // State Machine
    public StackFiniteStateMachine<ProgressManager> _fsm;
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
        //_currentMilestoneId = 0;
        //_currentMilestone = _milestones[_currentMilestoneId];

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



        // Hisn
        // Albacar
        // Almudayna
        // Ramiro II attack

        _conquestState = new(_milestones[^1], _fsm); // Last in list
        _foundationState = new(_milestones[0], _fsm, _conquestState); // Nothing built

        _fsm.SetInitialState(_foundationState);

        return _fsm;
    }
    #endregion

    #region PUBLIC METHODS
    public void SwitchToNextMilestone()
    {
        if (_fsm.SwitchToNextState())
        {
            //_currentMilestoneId++;
            //ChangeMilestone();
        }
    }

    public void SwitchToPreviousMilestone()
    {
        if (_fsm.SwitchToPreviousState())
        {
            //_currentMilestoneId--;
            //ChangeMilestone();
        }
    }

    public bool AtLastMilestone()
    {
        return _currentMilestone.Equals(_milestones[^1]);
    }

    public bool AtFirstMilestone()
    {
        return _currentMilestone.Equals(_milestones[0]);
    }

    public void InvokeOnMilestoneChanged()
    {
        OnMilestoneChanged?.Invoke(_currentMilestone.milestone);
    }
    #endregion

    #region PRIVATE METHODS
    // void ChangeMilestone()
    // {
    //     //_currentMilestone = _milestones[_currentMilestoneId];
    //     //_currentMilestone = _fsm.CurrentState._milestone;
    //     //OnMilestoneChanged?.Invoke(_currentMilestone.milestone);

    //     // Update current playable character
    //     GameManager.Instance.GetCurrentPlayableCharacter();
    // }
    #endregion
}
