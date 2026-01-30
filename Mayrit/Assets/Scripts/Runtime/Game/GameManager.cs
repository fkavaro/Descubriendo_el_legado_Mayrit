using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the game states and data. Singleton.
/// </summary>
public class GameManager : ABehaviourEntity<FiniteStateMachine<AGameState>>
{
    #region PROPERTY HELPERS
    public PlayableCharacter PlayableCharacter => _playableCharacter;
    public GameInputActions InputActions => _inputActions;
    public bool IsInMainMenuState => _fsm.IsCurrentState(_mainMenuState);
    public bool IsInGamePlayState => _fsm.IsCurrentState(_gamePlayState);
    public bool IsInPauseState => _fsm.IsCurrentState(_pauseState);
    #endregion

    #region EDITOR PROPERTIES
    [Header("Player")]
    [SerializeField] PlayableCharacter _playableCharacter;
    #endregion

    #region INTERNAL PROPERTIES
    GameInputActions _inputActions;
    FiniteStateMachine<AGameState> _fsm;
    MainMenu_GameState _mainMenuState;
    GamePlay_GameState _gamePlayState;
    Pause_GameState _pauseState;

    // Dependency Injection
    ScenesController _scenesController;
    ProgressManager _progressManager;
    #endregion

    #region INHERITED
    public override FiniteStateMachine<AGameState> DefineBehaviourSystemOnAwake()
    {
        _fsm = new(this);

        // States initialization
        _mainMenuState = new();
        _gamePlayState = new();
        _pauseState = new();

        // State AwakeState calls
        _mainMenuState.AwakeState();
        _gamePlayState.AwakeState();
        _pauseState.AwakeState();

        _fsm.SetInitialState(_mainMenuState);

        return _fsm;
    }
    #endregion

    #region LIFE CYCLE
    protected override void Awake()
    {
        // Only allow the registered GameManager to initialize
        var registered = ServiceLocator.Instance.Get<GameManager>();
        if (registered != null && registered != this)
        {
            Destroy(gameObject);
            return;
        }

        // Register to Service Locator
        ServiceLocator.Instance.Register(this);

        _scenesController = ServiceLocator.Instance.Get<ScenesController>();
        _scenesController.SceneChangedEvent += OnSceneChanged;

        _inputActions = new();

        base.Awake();
    }

    void OnDestroy()
    {
        // Unsubscribe from scene change event
        _scenesController.SceneChangedEvent -= OnSceneChanged;

        _inputActions = null;
    }
    #endregion

    #region PUBLIC METHODS
    public void SwitchToMainMenuState()
    {
        _fsm.SwitchState(_mainMenuState);

        // Unload Game Scene
        _scenesController.NewTransitionPlan()
            .Unload(SceneDatabase.Slot.Session)
            .Unload(SceneDatabase.Slot.Milestone)
            .WithOverlay()
            .ClearAssets()
            .Perform();
    }

    public void SwitchToGamePlayState()
    {
        // Load Game Scene
        _scenesController.NewTransitionPlan()
            .Load(SceneDatabase.Slot.Session, SceneDatabase.Name.GamePlayScene)
            .Load(SceneDatabase.Slot.Milestone, SceneDatabase.Name.Milestone, setActive: true)
            .WithOverlay()
            .ClearAssets()
            .Perform();

        _fsm.SwitchState(_gamePlayState);
    }

    public void SwitchToPauseState()
    {
        _fsm.SwitchState(_pauseState);
    }
    #endregion

    #region CALLBACK METHODS
    void OnMilestoneChanged(MilestoneMapping milestoneMapping)
    {
        _playableCharacter = milestoneMapping.PlayableCharacter; // TODO player will register itself in service locator
    }

    void OnSceneChanged(Dictionary<string, string> loadedScenes, List<string> unloadedSlots)
    {
        // In game play scene
        if (loadedScenes.ContainsValue(SceneDatabase.Name.GamePlayScene))
        {
            // Get dependencies from ServiceLocator
            _progressManager = ServiceLocator.Instance.Get<ProgressManager>();

            // Subscribe to events
            _progressManager.MilestoneChangedEvent += OnMilestoneChanged;
        }
    }
    #endregion
}
