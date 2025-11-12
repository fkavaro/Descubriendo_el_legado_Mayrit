using System;
using UnityEngine;

/// <summary>
/// Manages the progress states and data. Singleton.
/// </summary>
public class ProgressManager : ASingletonBehaviourEntity<ProgressManager, FiniteStateMachine>
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
    [Header("Milestones")]
    public bool _updateInInspector = true;
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

    #region INTERNAL PROPERTIES
    Milestone _lastValidatedMilestone;
    bool _lastUpdateInInspector;

    public event Action<Milestone> OnMilestoneChanged;
    public event Action<bool> OnEditorUpdateChanged;
    public event Action<float> OnTimeSet;

    FiniteStateMachine _fsm;
    public Vision_AProgressState _visionState;
    public Foundation_AProgressState _foundationState;
    public Albacar_AProgressState _albacarState;
    public Almudayna_AProgressState _almudaynaState;
    public RamiroIIAttack_AProgressState _ramiroIIState;
    public AlmanzorMeeting_AProgressState _almanzorState;
    public MaslamaSchool_AProgressState _schoolState;
    public Conquest_AProgressState _conquestState;
    #endregion

    #region INHERITED
    public override FiniteStateMachine InitializeBehaviourSystem()
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

        _fsm.SetInitialState(_visionState);

        return _fsm;
    }
    #endregion

    // Called when the script is loaded or a value is changed in the inspector
    void OnValidate()
    {
        if (Application.isPlaying)
            return;

#if UNITY_EDITOR
        // Invoke milestone changed event when _currentMilestone is changed in inspector
        // and _updateInInspector is true
        if (_lastValidatedMilestone != _currentMilestone)
        {
            _lastValidatedMilestone = _currentMilestone;

            if (_updateInInspector)
            {
                var milestone = _currentMilestone;

                // To avoid issues with re-entrancy
                UnityEditor.EditorApplication.delayCall += () => OnMilestoneChanged?.Invoke(milestone);
            }
        }

        // Invoke editor update changed event when _updateInInspector is changed in inspector
        if (_lastUpdateInInspector != _updateInInspector)
        {
            _lastUpdateInInspector = _updateInInspector;
            var update = _updateInInspector;
            UnityEditor.EditorApplication.delayCall += () => OnEditorUpdateChanged?.Invoke(update);
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

    public void InvokeOnMilestoneChanged()
    {
        OnMilestoneChanged?.Invoke(_currentMilestone);
    }

    public void InvokeOnTimeSet(float time)
    {
        OnTimeSet?.Invoke(time);
    }

    public Milestone_InformationSO GetCurrentMilestoneInfo()
    {
        Milestone_InformationSO info;

        switch (_currentMilestone)
        {
            case Milestone._1_Vision:
                info = _visionInformation;
                break;
            case Milestone._2_Foundation:
                info = _foundationInformation;
                break;
            case Milestone._3_Albacar:
                info = _albacarInformation;
                break;
            case Milestone._4_Almudayna:
                info = _almudaynaInformation;
                break;
            case Milestone._5_RamiroII:
                info = _ramiroAttackInformation;
                break;
            case Milestone._6_Almanzor:
                info = _almanzorInformation;
                break;
            case Milestone._7_School:
                info = _schoolInformation;
                break;
            case Milestone._8_Conquest:
                info = _conquestInformation;
                break;
            default:
                info = null;
                break;
        }
        return info;
    }
    #endregion
}
