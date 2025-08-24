using System;
using UnityEngine;

/// <summary>
/// Manages the progress states and data. Singleton.
/// </summary>
public class ProgressManager : Singleton<ProgressManager>, IBehaviourControllable
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

    #region EDITOR PROPERTIES
    [Header("Behaviour Controller Properties")]
    [Tooltip("Whether to show debug messages in the console or not")]
    [SerializeField] bool _debugMode = false;
    [Tooltip("Whether to update next frame or not")]
    [SerializeField] bool _isExecutionPaused = false;

    [Header("Milestone properties")]
    public Milestone _currentMilestone;

    [Space]
    public Milestone_InformationSO _visionInformation;
    public Milestone_InformationSO _foundationInformation;
    public Milestone_InformationSO _albacarInformation;
    public Milestone_InformationSO _almudaynaInformation;
    public Milestone_InformationSO _ramiroAttackInformation;
    public Milestone_InformationSO _almanzorInformation;
    public Milestone_InformationSO _schoolInformation;
    public Milestone_InformationSO _conquestInformation;
    #endregion

    #region PROPERTIES
    public event Action<Milestone> OnMilestoneChanged;
    public event Action<float> OnTimeSet;

    public string Name => gameObject.name;
    public bool DebugMode
    {
        get => _debugMode;
        set => _debugMode = value;
    }
    public bool IsExecutionPaused
    {
        get => _isExecutionPaused;
        set => _isExecutionPaused = value;
    }

    public ABehaviourController _behaviourController;
    public FiniteStateMachine _fsm;
    public Vision_AProgressState _visionState;
    public Foundation_AProgressState _foundationState;
    public Albacar_AProgressState _albacarState;
    public Almudayna_AProgressState _almudaynaState;
    public RamiroIIAttack_AProgressState _ramiroIIState;
    public AlmanzorMeeting_AProgressState _almanzorState;
    public MaslamaSchool_AProgressState _schoolState;
    public Conquest_AProgressState _conquestState;
    #endregion

    #region MONOBEHAVIOUR
    protected override void Awake()
    {
        // Singleton
        base.Awake();

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

        _behaviourController = new(_fsm);
        _behaviourController.Awake();
    }

    void Start()
    {
        _behaviourController.Start();
    }

    void Update()
    {
        _behaviourController.Update();
    }

    void LateUpdate()
    {
        _behaviourController.LateUpdate();
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
