using UnityEngine;
using System;
using System.Collections.Generic;

public class TutorialManager : ABehaviourEntity<StackFiniteStateMachine<TutorialState>>
{
    public bool HasCompletedTutorial => _hasCompletedTutorial;

    #region EDITOR PROPERTIES
    [Header("Tutorial Settings")]
    [SerializeField] bool _hasCompletedTutorial = false;

    [SerializeField] int _currentStepIndex = -1;
    [SerializeField] List<TutorialStepSO> _tutorialStepsData = new();
    #endregion

    #region INTERNAL PROPERTIES
    public event Action<bool> ShowPlayerFollowerEvent;
    public event Action<bool> ShowPointsOfInterestEvent;
    public event Action<bool> ShowCompassTutorialEvent;
    public event Action TutorialCompletedEvent;

    StackFiniteStateMachine<TutorialState> _fsm;
    ScenesController _scenesController;
    UISystem _uiSystem;
    #endregion

    #region INHERITED
    public override StackFiniteStateMachine<TutorialState> DefineBehaviourSystem()
    {
        _fsm = new(this);

        foreach (var data in _tutorialStepsData)
        {
            TutorialState newState = new(_uiSystem, data, _fsm);
            _fsm.AddStateToSequence(newState);
            newState.AwakeState();
        }

        _fsm.SwitchedStateEvent += OnSwitchedState;
        _fsm.SetInitialStateFromSequence(0);

        return _fsm;
    }
    #endregion

    #region LIFE CYCLE
    protected override void Awake()
    {
        ServiceLocator.Instance.Register(this);

        _uiSystem = FindFirstObjectByType<UISystem>();

        if (_uiSystem == null)
            Debug.LogWarning("TutorialManager: UISystem not found in the scene. Please ensure that a UISystem is present.");

        base.Awake();
    }

    protected override void Start()
    {
        _scenesController = ServiceLocator.Instance.Get<ScenesController>();
        _scenesController.ScenesLoadedFullyEvent += OnSceneLoadedFully;

        // base.Start(); when gameplay scene loaded, to start behaviour system
    }

    void OnDisable()
    {
        _scenesController.ScenesLoadedFullyEvent += OnSceneLoadedFully;

        ServiceLocator.Instance.Unregister(this);
    }
    #endregion

    #region CALLBACK METHODS
    void OnSwitchedState()
    {
        _currentStepIndex = _fsm.CurrentStateIndex;

        if (_currentStepIndex >= _tutorialStepsData.Count - 1)
        {
            _hasCompletedTutorial = true;
            GameSaveSystem.SaveTutorial(true);
            TutorialCompletedEvent?.Invoke();
        }

        ShowPlayerFollowerEvent?.Invoke(!_fsm.CurrentState.Data.VisualElementsToHide.Contains(UIElementsToHide.TutorialPlayerFollower));
        ShowPointsOfInterestEvent?.Invoke(!_fsm.CurrentState.Data.VisualElementsToHide.Contains(UIElementsToHide.TutorialModernVisualizationSwitch));
        ShowCompassTutorialEvent?.Invoke(!_fsm.CurrentState.Data.VisualElementsToHide.Contains(UIElementsToHide.TutorialCompass));
    }

    void OnSceneLoadedFully(Dictionary<SceneDatabase.SceneType, SceneDatabase.SceneName> loadedScenes, List<SceneDatabase.SceneType> unloadedScenes)
    {
        if (!loadedScenes.ContainsValue(SceneDatabase.SceneName.GameplayScene))
            return;

        _hasCompletedTutorial = GameSaveSystem.LoadTutorialCompletion();

        if (_hasCompletedTutorial)
            return;

        ShowPlayerFollowerEvent?.Invoke(false);
        ShowPointsOfInterestEvent?.Invoke(false);
        ShowCompassTutorialEvent?.Invoke(false);

        base.Start();
    }
    #endregion
}
