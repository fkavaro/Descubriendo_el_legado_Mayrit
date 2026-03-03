using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class TutorialManager : ABehaviourEntity<StackFiniteStateMachine<TutorialState>>
{
    #region EDITOR PROPERTIES
    [Header("Tutorial Settings")]
    [SerializeField] List<TutorialStepSO> _tutorialStepsData = new();
    #endregion

    #region INTERNAL PROPERTIES
    UIDocument _uiDocument;

    StackFiniteStateMachine<TutorialState> _fsm;

    ScenesController _scenesController;
    #endregion

    #region INHERITED
    public override StackFiniteStateMachine<TutorialState> DefineBehaviourSystemOnAwake()
    {
        _fsm = new(this);

        foreach (var data in _tutorialStepsData)
            _fsm.AddStateToSequence(new TutorialState(data, _uiDocument));

        return _fsm;
    }
    #endregion

    #region LIFE CYCLE
    protected override void Awake()
    {
        _uiDocument = GetComponent<UIDocument>();
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
        if (loadedScenes.ContainsValue(SceneDatabase.SceneName.GameplayScene))
        {
            _fsm.SetInitialStateFromSequence(0);
            base.Start();
        }
    }
    #endregion
}
