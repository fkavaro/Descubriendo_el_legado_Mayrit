using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ProgressManager : ABehaviourEntity<FiniteStateMachine<MilestoneState>>
{
    #region PROPERTY HELPERS
    public Milestone_DataSO CurrentMilestoneData => _milestonesData[_currentMilestoneIndex];
    public SceneDatabase.SceneName StoredMilestoneScene => _milestonesData[GetStoredMilestone()].SceneName;
    public bool WasCurrentMilestoneCompleted => _currentMilestoneIndex <= _storedMilestoneIndex;
    #endregion

    #region EDITOR PROPERTIES
    [Header("Debug tweaks")]
    [SerializeField] bool _canSkipTours = false;

    [Header("Milestones")]
    [Range(-1, 7)]
    [SerializeField] int _storedMilestoneIndex;
    [Range(0, 7)]
    [SerializeField] int _currentMilestoneIndex = 0;
    [SerializeField] List<Milestone_DataSO> _milestonesData = new();
    #endregion

    #region INTERNAL PROPERTIES
    public event Action<Milestone_DataSO> MilestoneChangedEvent;

    FiniteStateMachine<MilestoneState> _fsm;

    ScenesController _scenesController;
    TourManager _tourManager;
    #endregion

    #region INHERITED
    public override FiniteStateMachine<MilestoneState> DefineBehaviourSystem()
    {
        _fsm = new(this);

        foreach (var data in _milestonesData)
            _fsm.AddStateToSequence(new MilestoneState(data));

        return _fsm;
    }
    #endregion

    #region LIFE CYCLE
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
    public Milestone_DataSO SwitchToNextMilestone()
    {
        _fsm.SwitchToNextStateInSequence(out _currentMilestoneIndex);
        return CurrentMilestoneData;
    }

    public Milestone_DataSO SwitchToPreviousMilestone()
    {
        _fsm.SwitchToPreviousStateInSequence(out _currentMilestoneIndex);
        return CurrentMilestoneData;
    }
    public bool AtFirstMilestone() => _fsm.AtFistStateInSequence();
    public bool AtLastMilestone() => _fsm.AtLastStateInSequence();

    public bool IsCurrentMilestoneCompleted()
    {
        if (_tourManager == null || _tourManager.CurrentTour == null)
        {
            Debug.LogWarning("ProgressManager: No current tour found. Next milestone availability will always return false.");
            return false;
        }

        // TODO: full line when gold release
        return _canSkipTours || _tourManager.CurrentTour.IsCompleted; //Application.isPlaying && Application.isEditor || _tourManager.CurrentTour.HasBeenCompleted// TODO: full line when gold release
    }
    #endregion

    #region PRIVATE METHODS
    int GetStoredMilestone()
    {
        PlayerProgressData saveData = GameSaveSystem.LoadAllData();
        _storedMilestoneIndex = Mathf.Clamp(saveData.StoredMilestoneIndex, -1, _milestonesData.Count - 1); // Could be -1 if no valid data found
        _currentMilestoneIndex = _storedMilestoneIndex;

        if (_storedMilestoneIndex < _milestonesData.Count - 1) // Not at last milestone
            _currentMilestoneIndex++; // To load the next milestone to be completed

        if (DebugMode)
            Debug.Log($"[ProgressManager] Milestone Change index loaded {_currentMilestoneIndex} ({CurrentMilestoneData.Header}).");

        return _currentMilestoneIndex;
    }

    void UpdateStoredMilestoneIndex()
    {
        _storedMilestoneIndex = Mathf.Max(_storedMilestoneIndex, _currentMilestoneIndex);
    }

    void SaveProgress()
    {
        GameSaveSystem.SaveMilestoneIdx(_storedMilestoneIndex);
        if (DebugMode)
            Debug.Log($"ProgressManager: Progress saved. Highest completed milestone index: {_storedMilestoneIndex}");
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
        UpdateStoredMilestoneIndex();
        SaveProgress();
    }
    #endregion
}
