using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ProgressSystem : ABehaviourEntity<FiniteStateMachine<MilestoneState>>
{
    #region EDITOR PROPERTIES
    [Header("Milestones")]
    [SerializeField] List<Milestone_DataSO> _milestonesData = new();
    #endregion

    #region INTERNAL PROPERTIES
    public List<Milestone_DataSO> MilestonesData => _milestonesData;
    public event Action<Milestone_DataSO> MilestoneChangedEvent;

    FiniteStateMachine<MilestoneState> _fsm;

    ScenesController _scenesController;
    GameManager _gameManager;
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
        base.Awake();

        ServiceLocator.Instance.Register(this);
    }

    protected override void Start()
    {
        _scenesController = ServiceLocator.Instance.Get<ScenesController>();
        _scenesController.ScenesLoadedFullyEvent += OnScenesLoadedFully;

        _gameManager = ServiceLocator.Instance.Get<GameManager>();

        // base.Start(); when gameplay scene loaded, to start behaviour system
    }

    void OnDisable()
    {
        _scenesController.ScenesLoadedFullyEvent -= OnScenesLoadedFully;
        ServiceLocator.Instance.Unregister(this);
    }
    #endregion

    #region PUBLIC METHODS
    public int SwitchToNextMilestone() => _fsm.SwitchToNextStateInSequence();
    public int SwitchToPreviousMilestone() => _fsm.SwitchToPreviousStateInSequence();
    public bool AtFirstMilestone() => _fsm.AtFistStateInSequence();
    public bool AtLastMilestone() => _fsm.AtLastStateInSequence();
    #endregion

    #region CALLBACK METHODS
    void OnScenesLoadedFully(Dictionary<SceneDatabase.SceneType, SceneDatabase.SceneName> loadedScenes, List<SceneDatabase.SceneType> unloadedTypes)
    {
        // If gameplay scene loaded, start behaviour system
        if (loadedScenes.ContainsValue(SceneDatabase.SceneName.GameplayScene))
        {
            _fsm.SetInitialStateFromSequence(_gameManager.CurrentMilestoneIndex);
            base.Start();
        }

        // If milestone loaded, invoke event
        if (loadedScenes.TryGetValue(SceneDatabase.SceneType.Milestone, out var milestoneScene))
        {
            MilestoneChangedEvent?.Invoke(_gameManager.CurrentMilestoneData);

            if (DebugMode)
                Debug.Log($"[ProgressSystem] Milestone Change Event invoked for milestone index {_gameManager.CurrentMilestoneIndex} ({_gameManager.CurrentMilestoneData.Header}).");
        }
    }
    #endregion
}
