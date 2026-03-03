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

    void OnDisable()
    {
        _scenesController.ScenesLoadedFullyEvent -= OnScenesLoadedFully;
        ServiceLocator.Instance.Unregister(this);
    }
    #endregion

    #region CALLBACK METHODS
    void OnScenesLoadedFully(Dictionary<SceneDatabase.SceneType, SceneDatabase.SceneName> loadedScenes, List<SceneDatabase.SceneType> unloadedTypes)
    {
        if (loadedScenes.TryGetValue(SceneDatabase.SceneType.Milestone, out var milestoneScene))
            base.Start();
    }
    #endregion
}
