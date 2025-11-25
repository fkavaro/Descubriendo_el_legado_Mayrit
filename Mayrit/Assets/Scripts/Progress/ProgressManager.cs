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
    [Header("Scene changes")]
    [Tooltip("Wether to update scene at milestone changes in editor")]
    [SerializeField] bool _updateInEditor = true;

    [Header("Milestones")]
    [SerializeField] int _currentMilestoneIndex = 0;
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
            Debug.LogWarning("ProgressManager: No milestone mappings configured.");
            return null;
        }

        _fsm = new(this);

        // Susbcribe to state switch event to update current milestone
        _fsm.OnStateSwitchEvent += OnStateSwitch;

        // Build FSM states from configured milestone mappings
        foreach (var mapping in _milestoneMappings)
        {
            if (mapping == null) continue;
            var state = new MilestoneState(mapping);
            _fsm.AddStateToSequence(state);// Initial state is first in sequence
        }

        BuildMappingCache();

        return _fsm;
    }
    #endregion

    #region MONOBEHAVIOUR
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
        _milestonesMappings = new Dictionary<int, MilestoneMapping>();

        if (_milestoneMappings == null)
            return;

        foreach (var milestoneMapping in _milestoneMappings)
        {
            if (milestoneMapping == null || milestoneMapping.Data == null)
                continue;

            int idx = milestoneMapping.Data.Index;
            if (!_milestonesMappings.ContainsKey(idx))
                _milestonesMappings[idx] = milestoneMapping;
        }
    }

    MilestoneMapping GetMappingForIndex(int index)
    {
        if (_milestonesMappings == null)
            BuildMappingCache();

        if (_milestonesMappings != null && _milestonesMappings.TryGetValue(index, out MilestoneMapping mapping))
            return mapping;

        return null;
    }
    #endregion

    #region EVENT METHODS
    void OnStateSwitch()
    {
        if (_fsm?.CurrentState == null || _fsm.CurrentState.MilestoneMapping == null)
            return;

        _currentMilestoneIndex = _fsm.CurrentState.MilestoneMapping.Data.Index;
        OnMilestoneChangedEvent?.Invoke(CurrentMilestoneMapping);
    }
    #endregion
}
