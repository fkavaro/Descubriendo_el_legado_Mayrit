using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the progress states and data. Singleton.
/// </summary>
public class ProgressManager : ABehaviourEntity<FiniteStateMachine<MilestoneState>>
{
    #region PROPERTY HELPERS
    public Milestone_DataSO CurrentMilestoneMapping => _milestoneMappings[_currentMilestoneIndex];

    public int CurrentMilestoneIndex => _currentMilestoneIndex;
    #endregion

    #region EDITOR PROPERTIES
    [Header("Debug tweaks")]
    [SerializeField] bool _canSkipTours = false;

    // [Tooltip("Wether to update scene at milestone changes in editor")]
    // [SerializeField] bool _changesInEditor = false;
    [Header("Milestones")]
    [Range(0, 7)]
    [SerializeField] int _currentMilestoneIndex = 0;
    [SerializeField] Milestone_DataSO _currentMilestoneMapping;
    [SerializeField] List<Milestone_DataSO> _milestoneMappings = new();
    #endregion

    #region INTERNAL PROPERTIES
    public event Action<Milestone_DataSO> MilestoneChangedEvent;
    // public event Action<bool> OnEditorUpdateChangedEvent;

    // int _lastValidatedMilestoneIndex;
    // bool _lastUpdateInEditor;
    // Dictionary<int, MilestoneMapping> _milestonesMappings;

    FiniteStateMachine<MilestoneState> _fsm;

    ScenesController _scenesController;
    TourManager _tourManager;
    #endregion

    #region INHERITED
    public override FiniteStateMachine<MilestoneState> DefineBehaviourSystemOnAwake()
    {
        _fsm = new(this);

        // Build state for each milestone scene
        for (int i = 0; i < _milestoneMappings.Count; i++)
            _fsm.AddStateToSequence(new MilestoneState(_milestoneMappings[i]));

        _fsm.SwitchedStateEvent += OnStateSwitch;

        return _fsm;
    }
    #endregion

    #region LIFE CYCLE
    //     // Called when the script is loaded or a value is changed in the inspector
    //     void OnValidate()
    //     {
    // #if UNITY_EDITOR
    //         if (Application.isPlaying)
    //             return;

    //         // Invoke milestone changed event when _currentMilestone is changed in inspector
    //         // and _updateInInspector is true
    //         if (_lastValidatedMilestoneIndex != _currentMilestoneIndex)
    //         {
    //             _lastValidatedMilestoneIndex = _currentMilestoneIndex;

    //             if (_changesInEditor)
    //             {
    //                 // To avoid issues with re-entrancy
    //                 UnityEditor.EditorApplication.delayCall += () => MilestoneChangedEvent?.Invoke(CurrentMilestoneMapping);
    //             }
    //         }

    //         // Invoke editor update changed event when _updateInEditor is changed in inspector
    //         if (_lastUpdateInEditor != _changesInEditor)
    //         {
    //             _lastUpdateInEditor = _changesInEditor;
    //             bool update = _changesInEditor;

    //             UnityEditor.EditorApplication.delayCall += () => OnEditorUpdateChangedEvent?.Invoke(update);
    //         }
    // #endif
    //     }

    protected override void Awake()
    {
        ServiceLocator.Instance.Register(this);

        base.Awake();
    }

    protected override void Start()
    {
        _scenesController = ServiceLocator.Instance.Get<ScenesController>();
        _tourManager = ServiceLocator.Instance.Get<TourManager>();

        _scenesController.ScenesLoadedFullyEvent += OnScenesLoadedFully;
    }

    // TODO: this should be handled in superior abstract class
    void OnDisable()
    {
        ServiceLocator.Instance.Unregister(this);
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
    // MilestoneMapping GetMappingForIndex(int index)
    // {
    //     if (_milestonesMappings == null)
    //         BuildMappingCache();

    //     if (_milestonesMappings.TryGetValue(index, out MilestoneMapping mapping))
    //         return mapping;

    //     return null;
    // }

    public bool IsNextMilestoneAvailable()
    {
        if (_tourManager.CurrentTour == null)
        {
            Debug.LogWarning("[ProgressManager] No current tour available to check milestone availability.");
            return false;
        }

        bool canSkipInRuntime = _canSkipTours; //Application.isPlaying && Application.isEditor // TODO: full line when gold release
        bool tourCompleted = _tourManager.CurrentTour.IsCompleted;
        bool isNextMilestoneAvailable = !AtLastMilestone() && (canSkipInRuntime || tourCompleted);

        return isNextMilestoneAvailable;
    }
    #endregion

    #region CALLBACK METHODS
    void OnScenesLoadedFully(Dictionary<SceneDatabase.Slot, SceneDatabase.SceneName> scenesLoaded, List<SceneDatabase.Slot> slotUnloaded)
    {
        base.Start();
    }

    void OnStateSwitch()
    {
        if (!SceneManager.GetSceneByName(_fsm.CurrentState.Data.SceneName.ToString()).isLoaded)
        {
            Debug.LogWarning($"[ProgressManager] Current milestone scene {_fsm.CurrentState.Data.SceneName} is not loaded!");
            return;
        }

        MilestoneChangedEvent?.Invoke(CurrentMilestoneMapping);
    }
    #endregion
}
