using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ProgressManager : ABehaviourEntity<FiniteStateMachine<MilestoneState>>
{
    #region PROPERTY HELPERS
    public Milestone_DataSO CurrentMilestoneData => _milestonesData[_currentMilestoneIndex];
    public SceneDatabase.SceneName StoredMilestoneScene => _milestonesData[GetStoredMilestone()].SceneName;

    public int HighestCompletedMilestoneIndex => _highestCompletedMilestoneIndex;
    #endregion

    #region EDITOR PROPERTIES
    [Header("Debug tweaks")]
    [SerializeField] bool _canSkipTours = false;

    // TODO: remove eventually
    // [Tooltip("Wether to update scene at milestone changes in editor")]
    // [SerializeField] bool _updateInEditor = false;
    [Header("Milestones")]
    [Range(0, 7)]
    [SerializeField] int _currentMilestoneIndex = 0;
    [SerializeField] List<Milestone_DataSO> _milestonesData = new();
    #endregion

    #region INTERNAL PROPERTIES
    public event Action<Milestone_DataSO> MilestoneChangedEvent;

    // TODO: remove eventually
    // public event Action<bool> OnEditorUpdateChangedEvent;
    // int _lastValidatedMilestoneIndex;
    // bool _lastUpdateInEditor;

    FiniteStateMachine<MilestoneState> _fsm;

    ScenesController _scenesController;
    TourManager _tourManager;
    int _highestCompletedMilestoneIndex;
    #endregion

    #region INHERITED
    public override FiniteStateMachine<MilestoneState> DefineBehaviourSystemOnAwake()
    {
        _fsm = new(this);

        // Build state for each milestone scene
        for (int i = 0; i < _milestonesData.Count; i++)
            _fsm.AddStateToSequence(new MilestoneState(_milestonesData[i]));

        return _fsm;
    }
    #endregion

    #region LIFE CYCLE
    // TODO remove eventually
    //     // Called when the script is loaded or a value is changed in the inspector
    //     void OnValidate()
    //     {
    // #if UNITY_EDITOR
    //         if (Application.isPlaying)
    //             return;

    //         // Invoke milestone changed event when _currentMilestoneIndex is changed in inspector
    //         // and _updateInInspector is true
    //         if (_lastValidatedMilestoneIndex != _currentMilestoneIndex)
    //         {
    //             _lastValidatedMilestoneIndex = _currentMilestoneIndex;

    //             if (_updateInEditor)
    //             {
    //                 _currentMilestoneMapping = _milestoneMappings[_currentMilestoneIndex];
    //                 // To avoid issues with re-entrancy
    //                 UnityEditor.EditorApplication.delayCall += () => MilestoneChangedEvent?.Invoke(CurrentMilestoneMapping);
    //             }
    //         }

    //         // Invoke editor update changed event when _updateInEditor is changed in inspector
    //         if (_lastUpdateInEditor != _updateInEditor)
    //         {
    //             _lastUpdateInEditor = _updateInEditor;

    //             if (_updateInEditor)
    //                 _currentMilestoneMapping = _milestoneMappings[_currentMilestoneIndex];
    //             else
    //                 _currentMilestoneMapping = null;

    //             UnityEditor.EditorApplication.delayCall += () => OnEditorUpdateChangedEvent?.Invoke(_updateInEditor);
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
        _scenesController.SceneLoadedPartiallyEvent += OnSceneLoadedPartially;
        _scenesController.ScenesLoadedFullyEvent += OnScenesLoadedFully;

        // base.Start(); when gameplay scene loaded, to start behaviour system
    }

    void OnDisable()
    {
        _scenesController.SceneLoadedPartiallyEvent -= OnSceneLoadedPartially;
        _scenesController.ScenesLoadedFullyEvent -= OnScenesLoadedFully;
        ServiceLocator.Instance.Unregister(this);
    }
    #endregion

    #region PUBLIC METHODS
    public void SwitchToNextMilestone() => _fsm.SwitchToNextStateInSequence(out _currentMilestoneIndex);
    public void SwitchToPreviousMilestone() => _fsm.SwitchToPreviousStateInSequence(out _currentMilestoneIndex);
    public bool AtFirstMilestone() => _fsm.AtFistStateInSequence();
    public bool AtLastMilestone() => _fsm.AtLastStateInSequence();

    public bool IsNextMilestoneAvailable()
    {
        if (_tourManager == null || _tourManager.CurrentTour == null)
        {
            if (DebugMode)
                Debug.LogWarning("ProgressManager: No current tour found. Next milestone availability will always return false.");
            return false;
        }

        bool canSkipInRuntime = _canSkipTours; //Application.isPlaying && Application.isEditor // TODO: full line when gold release
        bool tourCompleted = _tourManager.CurrentTour.IsCompleted;
        bool isNextMilestoneAvailable = !AtLastMilestone() && (canSkipInRuntime || tourCompleted);

        return isNextMilestoneAvailable;
    }
    #endregion

    #region PRIVATE METHODS
    // TODO: remove eventually
    // MilestoneMapping GetMappingForIndex(int index)
    // {
    //     if (_milestonesMappings == null)
    //         BuildMappingCache();

    //     if (_milestonesMappings.TryGetValue(index, out MilestoneMapping mapping))
    //         return mapping;

    //     return null;
    // }

    int GetStoredMilestone()
    {
        PlayerProgressData saveData = GameSaveSystem.Load();
        _highestCompletedMilestoneIndex = saveData.HighestCompletedMilestoneIndex; // Could be -1 if no valid data found
        _currentMilestoneIndex = Mathf.Clamp(saveData.HighestCompletedMilestoneIndex, 0, _milestonesData.Count - 1);

        if (DebugMode)
            Debug.Log($"[ProgressManager] Milestone Change index loaded {_currentMilestoneIndex} ({CurrentMilestoneData.Header}).");

        return _currentMilestoneIndex;
    }

    void UpdateHighestCompletedMilestone()
    {
        _highestCompletedMilestoneIndex = Mathf.Max(_highestCompletedMilestoneIndex, _currentMilestoneIndex);
    }

    void SaveProgress()
    {
        GameSaveSystem.Save(_highestCompletedMilestoneIndex);
        if (DebugMode)
            Debug.Log($"ProgressManager: Progress saved. Highest completed milestone index: {_highestCompletedMilestoneIndex}");
    }
    #endregion

    #region CALLBACK METHODS
    void OnSceneLoadedPartially(SceneDatabase.SceneType type, SceneDatabase.SceneName name)
    {
        if (name == SceneDatabase.SceneName.GameplayScene)
        {
            _tourManager = ServiceLocator.Instance.Get<TourManager>();
            _tourManager.TourCompletedEvent += OnTourCompleted;
        }
    }

    void OnScenesLoadedFully(Dictionary<SceneDatabase.SceneType, SceneDatabase.SceneName> loadedScenes, List<SceneDatabase.SceneType> unloadedTypes)
    {
        // If gameplay scene loaded, start behaviour system
        if (loadedScenes.ContainsValue(SceneDatabase.SceneName.GameplayScene))
        {
            _fsm.SetInitialStateFromSequence(_currentMilestoneIndex);
            base.Start();
        }

        // If milestone loaded, invoke event
        if (loadedScenes.TryGetValue(SceneDatabase.SceneType.Milestone, out var milestoneScene))
        {
            MilestoneChangedEvent?.Invoke(CurrentMilestoneData);

            if (DebugMode)
                Debug.Log($"[ProgressManager] Milestone Change Event invoked for milestone index {_currentMilestoneIndex} ({CurrentMilestoneData.Header}).");
        }
    }

    void OnTourCompleted(Tour tour)
    {
        UpdateHighestCompletedMilestone();
        SaveProgress();
    }
    #endregion
}
