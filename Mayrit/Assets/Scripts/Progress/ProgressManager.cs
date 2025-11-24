using System;
using UnityEngine;

/// <summary>
/// Manages the progress states and data. Singleton.
/// </summary>
public class ProgressManager : ASingletonBehaviourEntity<ProgressManager, FiniteStateMachine<AProgressState>>
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

    #region PROPERTY HELPERS
    public Milestone CurrentMilestone
    {
        get => _currentMilestone;
        set => _currentMilestone = value;
    }
    #endregion

    #region EDITOR PROPERTIES
    [Header("Milestones")]
    [SerializeField] bool _updateInInspector = true;
    [SerializeField] Milestone _currentMilestone;

    [Space] // TODO: make list?
    public Milestone_InformationSO _visionInformation;
    public Milestone_InformationSO _foundationInformation;
    public Milestone_InformationSO _albacarInformation;
    public Milestone_InformationSO _almudaynaInformation;
    public Milestone_InformationSO _ramiroAttackInformation;
    public Milestone_InformationSO _almanzorInformation;
    public Milestone_InformationSO _schoolInformation;
    public Milestone_InformationSO _conquestInformation;
    #endregion

    #region INTERNAL PROPERTIES
    public event Action<Milestone> OnMilestoneChangedEvent;
    public event Action<bool> OnEditorUpdateChangedEvent;
    public event Action<float> OnTimeSetEvent;

    Milestone _lastValidatedMilestone;
    bool _lastUpdateInInspector;

    FiniteStateMachine<AProgressState> _fsm;
    #endregion

    #region INHERITED
    public override FiniteStateMachine<AProgressState> InitializeBehaviourSystem()
    {
        _fsm = new(this);

        // Susbcribe to state switch event to update current milestone
        _fsm.OnStateSwitchEvent += OnStateSwitch;

        // States initialization
        Vision_AProgressState _visionState = new(Milestone._1_Vision, _visionInformation);
        Foundation_AProgressState _foundationState = new(Milestone._2_Foundation, _foundationInformation);
        Albacar_AProgressState _albacarState = new(Milestone._3_Albacar, _albacarInformation);
        Almudayna_AProgressState _almudaynaState = new(Milestone._4_Almudayna, _almudaynaInformation);
        RamiroIIAttack_AProgressState _ramiroIIState = new(Milestone._5_RamiroII, _ramiroAttackInformation);
        AlmanzorMeeting_AProgressState _almanzorState = new(Milestone._6_Almanzor, _almanzorInformation);
        MaslamaSchool_AProgressState _schoolState = new(Milestone._7_School, _schoolInformation);
        Conquest_AProgressState _conquestState = new(Milestone._8_Conquest, _conquestInformation);

        // Add states to the FSM sequence
        _fsm.AddStateToSequence(_visionState);
        _fsm.AddStateToSequence(_foundationState);
        _fsm.AddStateToSequence(_albacarState);
        _fsm.AddStateToSequence(_almudaynaState);
        _fsm.AddStateToSequence(_ramiroIIState);
        _fsm.AddStateToSequence(_almanzorState);
        _fsm.AddStateToSequence(_schoolState);
        _fsm.AddStateToSequence(_conquestState);

        _fsm.SetInitialState(_visionState);

        return _fsm;
    }
    #endregion

    // Called when the script is loaded or a value is changed in the inspector
    void OnValidate()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
            return;

        // Invoke milestone changed event when _currentMilestone is changed in inspector
        // and _updateInInspector is true
        if (_lastValidatedMilestone != _currentMilestone)
        {
            _lastValidatedMilestone = _currentMilestone;

            if (_updateInInspector)
            {
                Milestone milestone = _currentMilestone;

                // To avoid issues with re-entrancy
                UnityEditor.EditorApplication.delayCall += () => OnMilestoneChangedEvent?.Invoke(milestone);
            }
        }

        // Invoke editor update changed event when _updateInInspector is changed in inspector
        if (_lastUpdateInInspector != _updateInInspector)
        {
            _lastUpdateInInspector = _updateInInspector;
            bool update = _updateInInspector;

            UnityEditor.EditorApplication.delayCall += () => OnEditorUpdateChangedEvent?.Invoke(update);
        }
#endif
    }

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

    public Milestone_InformationSO GetCurrentMilestoneInfo()
    {
        Milestone_InformationSO info = _currentMilestone switch
        {
            Milestone._1_Vision => _visionInformation,
            Milestone._2_Foundation => _foundationInformation,
            Milestone._3_Albacar => _albacarInformation,
            Milestone._4_Almudayna => _almudaynaInformation,
            Milestone._5_RamiroII => _ramiroAttackInformation,
            Milestone._6_Almanzor => _almanzorInformation,
            Milestone._7_School => _schoolInformation,
            Milestone._8_Conquest => _conquestInformation,
            _ => null,
        };
        return info;
    }
    #endregion

    #region EVENT METHODS
    void OnStateSwitch()
    {
        _currentMilestone = _fsm.CurrentState._milestone;
        OnMilestoneChangedEvent?.Invoke(_currentMilestone);
        OnTimeSetEvent?.Invoke(_fsm.CurrentState._informationSO.WantedTime);
    }
    #endregion
}
