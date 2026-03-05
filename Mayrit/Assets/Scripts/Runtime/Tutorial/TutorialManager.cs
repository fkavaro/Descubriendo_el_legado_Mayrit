using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class TutorialManager : ABehaviourEntity<StackFiniteStateMachine<TutorialState>>
{
    #region EDITOR PROPERTIES
    [Header("Tutorial Settings")]
    [SerializeField] bool _hasCompletedTutorial = false;
    [SerializeField] UIManager _uiManager;
    [SerializeField] int _currentStepIndex = -1;
    [SerializeField] List<TutorialStepSO> _tutorialStepsData = new();
    #endregion

    #region INTERNAL PROPERTIES
    public event Action<bool> ShowPlayerFollowerEvent;
    public event Action<bool> ShowLandmarkVisualsEvent;
    public event Action TutorialCompletedEvent;

    StackFiniteStateMachine<TutorialState> _fsm;
    ScenesController _scenesController;
    #endregion

    #region INHERITED
    public override StackFiniteStateMachine<TutorialState> DefineBehaviourSystemOnAwake()
    {
        _fsm = new(this);

        foreach (var data in _tutorialStepsData)
        {
            TutorialState newState = new(data, _uiManager, _fsm);
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

        _hasCompletedTutorial = GameSaveSystem.LoadTutorialCompletion();

        base.Awake();
    }

    protected override void Start()
    {
        _scenesController = ServiceLocator.Instance.Get<ScenesController>();
        _scenesController.SceneLoadedPartiallyEvent += OnSceneLoadedPartially;

        // base.Start(); when gameplay scene loaded, to start behaviour system
    }

    void OnDisable()
    {
        _scenesController.SceneLoadedPartiallyEvent -= OnSceneLoadedPartially;

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

        if (_fsm.CurrentState.Data.VisualElementsToHide.Contains(UIElementsToHide.TutorialPlayerFollower))
            ShowPlayerFollowerEvent?.Invoke(false);
        else
            ShowPlayerFollowerEvent?.Invoke(true);

        if (_fsm.CurrentState.Data.VisualElementsToHide.Contains(UIElementsToHide.TutorialSwitches))
            ShowLandmarkVisualsEvent?.Invoke(false);
        else
            ShowLandmarkVisualsEvent?.Invoke(true);
    }

    private void OnSceneLoadedPartially(SceneDatabase.SceneType type, SceneDatabase.SceneName name)
    {
        if (name != SceneDatabase.SceneName.GameplayScene)
            return;

        _hasCompletedTutorial = GameSaveSystem.LoadTutorialCompletion();

        if (_hasCompletedTutorial)
            return;

        base.Start();
    }
    #endregion
}
