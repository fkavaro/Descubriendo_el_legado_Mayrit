using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the progress states and data. Singleton.
/// </summary>
public class ProgressManager : ABehaviourEntity<FiniteStateMachine<MilestoneState>>
{
    #region PROPERTY HELPERS
    public MilestoneMapping CurrentMilestoneMapping => GetMappingForIndex(_currentMilestoneIndex);
    public int CurrentMilestoneIndex => _currentMilestoneIndex;
    #endregion

    #region EDITOR PROPERTIES
    [Header("Scene changes")]
    [Tooltip("Wether to update scene at milestone changes in editor")]
    [SerializeField] bool _updateInEditor = true;

    [Header("Milestones")]
    [Range(0, 7)]
    [SerializeField] int _currentMilestoneIndex = 0;
    [SerializeField] MilestoneMapping _currentMilestoneMapping;
    [SerializeField] List<MilestoneMapping> _milestoneMappings = new();
    #endregion

    #region INTERNAL PROPERTIES
    public event Action<MilestoneMapping> OnMilestoneChangedEvent;
    public event Action<bool> OnEditorUpdateChangedEvent;

    int _lastValidatedMilestoneIndex;
    bool _lastUpdateInEditor;
    Dictionary<int, MilestoneMapping> _milestonesMappings;

    FiniteStateMachine<MilestoneState> _fsm;
    #endregion

    #region INHERITED
    public override FiniteStateMachine<MilestoneState> InitializeBehaviourSystem()
    {
        if (_milestoneMappings == null || _milestoneMappings.Count == 0)
        {
            Debug.LogError("    ProgressManager: No milestone mappings configured.");
            return null;
        }

        _fsm = new(this);

        // Build FSM states from configured milestone mappings
        foreach (MilestoneMapping mapping in _milestoneMappings)
        {
            if (mapping == null)
            {
                Debug.LogError("    ProgressManager: Null milestone mapping found in configuration.");
                return null;
            }

            MilestoneState state = new(mapping);
            _fsm.AddStateToSequence(state); // Initial state is first in sequence
        }

        BuildMappingCache();

        // Susbcribe to state switch event to update current milestone
        _fsm.OnStateSwitchEvent += OnStateSwitch;

        return _fsm;
    }
    #endregion

    #region LIFE CYCLE
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

        // Invoke editor update changed event when _updateInEditor is changed in inspector
        if (_lastUpdateInEditor != _updateInEditor)
        {
            _lastUpdateInEditor = _updateInEditor;
            bool update = _updateInEditor;

            UnityEditor.EditorApplication.delayCall += () => OnEditorUpdateChangedEvent?.Invoke(update);
        }
#endif
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
        _milestonesMappings = new();

        foreach (MilestoneMapping milestoneMapping in _milestoneMappings)
        {
            if (milestoneMapping.Data == null)
            {
                Debug.LogError("    ProgressManager: milestone mapping with null data found in configuration.");
                return;
            }

            int idx = milestoneMapping.Index;
            if (!_milestonesMappings.ContainsKey(idx))
                _milestonesMappings[idx] = milestoneMapping;
        }
    }

    MilestoneMapping GetMappingForIndex(int index)
    {
        if (_milestonesMappings == null)
            BuildMappingCache();

        if (_milestonesMappings.TryGetValue(index, out MilestoneMapping mapping))
            return mapping;

        return null;
    }
    #endregion

    #region EVENT METHODS
    void OnStateSwitch()
    {
        if (_fsm?.CurrentState == null || _fsm.CurrentState.MilestoneMapping == null)
            return;

        _currentMilestoneIndex = _fsm.CurrentState.MilestoneMapping.Index;
        _currentMilestoneMapping = CurrentMilestoneMapping;

        if (_currentMilestoneMapping == null)
        {
            Debug.LogError("    ProgressManager: Current milestone mapping is null on state switch.");
            return;
        }

        if (_currentMilestoneMapping.Data == null || _currentMilestoneMapping.PlayableCharacter == null || _currentMilestoneMapping.Tour == null)
        {
            if (_currentMilestoneMapping.Data == null)
                Debug.LogError("    ProgressManager: Current milestone data is null on state switch.");
            if (_currentMilestoneMapping.PlayableCharacter == null)
                Debug.LogError("    ProgressManager: Current milestone has no playable character assigned on state switch.");
            if (_currentMilestoneMapping.Tour == null)
                Debug.LogError("    ProgressManager: Current milestone has no tour assigned on state switch.");
            return;
        }

        OnMilestoneChangedEvent?.Invoke(CurrentMilestoneMapping);
    }
    #endregion
}
