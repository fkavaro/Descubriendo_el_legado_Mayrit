using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class TutorialManager : ABehaviourEntity<StackFiniteStateMachine<TutorialState>>
{
    #region EDITOR PROPERTIES
    [Header("Tutorial Settings")]
    [SerializeField] UIDocument _uiDocument;
    [SerializeField] List<TutorialStepSO> _tutorialStepsData = new();
    #endregion

    #region INTERNAL PROPERTIES
    int _currentStepIndex = 0;
    ATutorialStepConditionSO _currentCondition;
    StackFiniteStateMachine<TutorialState> _fsm;
    ScenesController _scenesController;
    #endregion

    #region INHERITED
    public override StackFiniteStateMachine<TutorialState> DefineBehaviourSystemOnAwake()
    {
        _fsm = new(this);

        foreach (var data in _tutorialStepsData)
        {
            TutorialState newState = new(data, _uiDocument);
            _fsm.AddStateToSequence(newState);
            newState.AwakeState();
        }

        _fsm.SetInitialStateFromSequence(0);

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
        _scenesController.ScenesLoadedFullyEvent += OnScenesLoadedFully;

        // base.Start(); when gameplay scene loaded, to start behaviour system
    }

    protected override void Update()
    {
        if (_currentCondition != null)
            _currentCondition.Tick(Time.deltaTime);
    }

    void OnDisable()
    {
        if (_scenesController != null)
            _scenesController.ScenesLoadedFullyEvent -= OnScenesLoadedFully;

        ServiceLocator.Instance.Unregister(this);
        StopCurrentCondition();
    }
    #endregion

    #region PRIVATE METHODS
    void StartCurrentCondition()
    {
        if (_currentStepIndex < 0 || _currentStepIndex >= _tutorialStepsData.Count)
            return;

        ATutorialStepConditionSO condition = _tutorialStepsData[_currentStepIndex].CompletionCondition;
        if (condition == null)
        {
            Debug.LogWarning($"Tutorial step {_currentStepIndex} has no completion condition.");
            return;
        }

        // runtime clone avoids shared SO state across runs/steps
        _currentCondition = Instantiate(condition);
        _currentCondition.Completed += OnCurrentStepCompleted;

        _currentCondition.SetUIDocument(_uiDocument);

        _currentCondition.BeginListening();
    }

    void StopCurrentCondition()
    {
        if (_currentCondition == null) return;

        _currentCondition.Completed -= OnCurrentStepCompleted;
        _currentCondition.EndListening();
        Destroy(_currentCondition);
        _currentCondition = null;
    }

    void OnCurrentStepCompleted()
    {
        StopCurrentCondition();

        _fsm.SwitchToNextStateInSequence(out int nextIndex);
        _currentStepIndex = nextIndex;

        StartCurrentCondition();
    }
    #endregion

    #region CALLBACK METHODS
    void OnScenesLoadedFully(Dictionary<SceneDatabase.SceneType, SceneDatabase.SceneName> loadedScenes, List<SceneDatabase.SceneType> unloadedTypes)
    {
        if (!loadedScenes.TryGetValue(SceneDatabase.SceneType.Milestone, out var milestoneScene))
            return;

        base.Start();
        _currentStepIndex = 0;
        StartCurrentCondition();
    }
    #endregion


}
