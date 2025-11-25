using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the progress states and data. Singleton.
/// </summary>
public class ProgressManager : ASingletonBehaviourEntity<ProgressManager, FiniteStateMachine<MilestoneState>>
{
    #region PROPERTY HELPERS
    public MilestoneMapping CurrentMilestoneMapping => GetMappingForIndex(_currentMilestoneIndex);
    public int CurrentMilestoneIndex => _currentMilestoneIndex;
    #endregion

    #region EDITOR PROPERTIES
    [Header("Milestones")]
    [SerializeField] bool _updateInEditor = true;
    [SerializeField] int _currentMilestoneIndex = 0;
    [SerializeField] List<MilestoneMapping> _milestoneMappings = new();
    #endregion

    #region INTERNAL PROPERTIES
    public event Action<MilestoneMapping> OnMilestoneChangedEvent;
    public event Action<bool> OnEditorUpdateChangedEvent;

    int _lastValidatedMilestoneIndex;
    bool _lastUpdateInInspector;
    Dictionary<int, MilestoneMapping> _mappingByIndex;

    FiniteStateMachine<MilestoneState> _fsm;
    #endregion

    #region INHERITED
    public override FiniteStateMachine<MilestoneState> InitializeBehaviourSystem()
    {
        _fsm = new(this);

        // Susbcribe to state switch event to update current milestone
        _fsm.OnStateSwitchEvent += OnStateSwitch;

        // States initialization
        MilestoneState _visionState = new(_milestoneMappings[0]);
        MilestoneState _foundationState = new(_milestoneMappings[1]);
        MilestoneState _albacarState = new(_milestoneMappings[2]);
        MilestoneState _almudaynaState = new(_milestoneMappings[3]);
        MilestoneState _ramiroIIState = new(_milestoneMappings[4]);
        MilestoneState _almanzorState = new(_milestoneMappings[5]);
        MilestoneState _schoolState = new(_milestoneMappings[6]);
        MilestoneState _conquestState = new(_milestoneMappings[7]);

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

        BuildMappingCache();
        TourManager.Instance.OnTourCompletedEvent += OnTourCompleted;

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
        if (_lastValidatedMilestoneIndex != _currentMilestoneIndex)
        {
            _lastValidatedMilestoneIndex = _currentMilestoneIndex;

            if (_updateInEditor)
            {
                // To avoid issues with re-entrancy
                UnityEditor.EditorApplication.delayCall += () => OnMilestoneChangedEvent?.Invoke(CurrentMilestoneMapping);
            }
        }

        // Invoke editor update changed event when _updateInInspector is changed in inspector
        if (_lastUpdateInInspector != _updateInEditor)
        {
            _lastUpdateInInspector = _updateInEditor;
            bool update = _updateInEditor;

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

    public bool AtFirstMilestone()
    {
        return _fsm.AtFistStateInSequence();
    }

    public bool AtLastMilestone()
    {
        return _fsm.AtLastStateInSequence();
    }
    #endregion

    #region PRIVATE METHODS
    void BuildMappingCache()
    {
        _mappingByIndex = new Dictionary<int, MilestoneMapping>();

        if (_milestoneMappings == null)
            return;

        foreach (var milestoneMapping in _milestoneMappings)
        {
            if (milestoneMapping == null || milestoneMapping.Data == null)
                continue;

            int idx = milestoneMapping.Data.Index;
            if (!_mappingByIndex.ContainsKey(idx))
                _mappingByIndex[idx] = milestoneMapping;
        }
    }

    MilestoneMapping GetMappingForIndex(int index)
    {
        if (_mappingByIndex == null)
            BuildMappingCache();

        if (_mappingByIndex != null && _mappingByIndex.TryGetValue(index, out MilestoneMapping mapping))
            return mapping;

        return null;
    }
    #endregion

    #region EVENT METHODS
    void OnStateSwitch()
    {
        _currentMilestoneIndex = _fsm.CurrentState.MilestoneMapping.Data.Index;
        OnMilestoneChangedEvent?.Invoke(CurrentMilestoneMapping);
    }

    void OnTourCompleted()
    {
        throw new NotImplementedException();
    }
    #endregion
}
