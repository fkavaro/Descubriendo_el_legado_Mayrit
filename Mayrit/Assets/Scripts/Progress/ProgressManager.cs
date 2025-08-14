using System;
using UnityEngine;

public class ProgressManager : Singleton<ProgressManager>
{
    public enum Milestone
    {
        _1_Vision,
        _2_Foundation,
        _3_Albacar,
        _4_Almudayna,
        _5_RamiroII,
        _6_Almanzor,
        _7_School,
        _8_Conquest,
    }

    #region PUBLIC PROPERTIES
    public event Action<Milestone> OnMilestoneChanged;
    public event Action<float> OnTimeSet;

    [Header("Milestone properties")]
    public Milestone _currentMilestone;

    [Space(10)]
    public InformationSO _visionInformation;
    public InformationSO _foundationInformation;
    public InformationSO _albacarInformation;
    public InformationSO _almudaynaInformation;
    public InformationSO _ramiroAttackInformation;
    public InformationSO _almanzorInformation;
    public InformationSO _schoolInformation;
    public InformationSO _conquestInformation;

    // State Machine
    public FiniteStateMachine<ProgressManager> _fsm;
    public Vision_AProgressState _visionState;
    public Foundation_AProgressState _foundationState;
    public Albacar_AProgressState _albacarState;
    public Almudayna_AProgressState _almudaynaState;
    public RamiroIIAttack_AProgressState _ramiroIIState;
    public AlmanzorMeeting_AProgressState _almanzorState;
    public MaslamaSchool_AProgressState _schoolState;
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
        // Notify listeners about the initial milestone
        //OnMilestoneChanged?.Invoke(_currentMilestone);

        // Set current playable character
        //GameManager.Instance.GetCurrentPlayableCharacter();
    }

    protected override void OnUpdate()
    {

    }

    protected override ADecisionSystem<ProgressManager> CreateDecisionSystem()
    {
        _fsm = new(this);

        // States initialization
        _visionState = new(Milestone._1_Vision, _visionInformation, _fsm);
        _foundationState = new(Milestone._2_Foundation, _foundationInformation, _fsm);
        _albacarState = new(Milestone._3_Albacar, _albacarInformation, _fsm);
        _almudaynaState = new(Milestone._4_Almudayna, _almudaynaInformation, _fsm);
        _ramiroIIState = new(Milestone._5_RamiroII, _ramiroAttackInformation, _fsm);
        _almanzorState = new(Milestone._6_Almanzor, _almanzorInformation, _fsm);
        _schoolState = new(Milestone._7_School, _schoolInformation, _fsm);
        _conquestState = new(Milestone._8_Conquest, _conquestInformation, _fsm);

        //_fsm.SetInitialState(_visionState);

        return _fsm;
    }
    #endregion

    #region PUBLIC METHODS
    public void SwitchToNextMilestone()
    {
        _fsm.SwitchToNextStateInSequence();
    }

    public void SwitchToPreviousMilestone()
    {
        _fsm.SwitchToPreviousStateInSequence();
    }

    public bool AtLastMilestone()
    {
        return _currentMilestone.Equals(Milestone._8_Conquest);
    }

    public bool AtFirstMilestone()
    {
        return _currentMilestone.Equals(Milestone._1_Vision);
    }

    public void InvokeOnMilestoneChanged()
    {
        OnMilestoneChanged?.Invoke(_currentMilestone);
    }

    public void InvokeOnTimeSet(float time)
    {
        OnTimeSet?.Invoke(time);
    }
    #endregion

    #region PRIVATE METHODS

    #endregion
}
