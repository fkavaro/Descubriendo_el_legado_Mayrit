using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the game states and data.
/// </summary>
public class GameManager : ABehaviourEntity<FiniteStateMachine<AGameState>>
{
    #region PROPERTY HELPERS
    public GameInputActions InputActions => _inputActions;
    public bool IsInMainMenuState => _fsm.IsCurrentState(_mainMenuState);
    public bool IsInGamePlayState => _fsm.IsCurrentState(_gamePlayState);
    public bool IsInPauseState => _fsm.IsCurrentState(_pauseState);
    #endregion

    #region INTERNAL PROPERTIES
    GameInputActions _inputActions;
    FiniteStateMachine<AGameState> _fsm;
    MainMenu_GameState _mainMenuState;
    GamePlay_GameState _gamePlayState;
    Pause_GameState _pauseState;

    // Dependency Injection
    ScenesController _scenesController;
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

        // Load main menu scene
        _scenesController.NewTransitionPlan()
            .Load(SceneDatabase.Slot.Session, SceneDatabase.SceneName.MainMenuScene, setActive: true)
            .Perform();

        return _fsm;
    }
    #endregion

    #region LIFE CYCLE
    protected override void Awake()
    {
        _scenesController = ServiceLocator.Instance.Get<ScenesController>();

        _inputActions = new();

        base.Awake();

        ServiceLocator.Instance.Register(this);
    }

    // TODO: this should be handled in superior abstract class
    void OnDisable()
    {
        ServiceLocator.Instance.Unregister(this);
    }

    void OnDestroy()
    {
        _inputActions = null;
    }
    #endregion

    #region PUBLIC METHODS
    public void SwitchToMainMenuState()
    {
        // Load main menu scene
        _scenesController.NewTransitionPlan()
            .Load(SceneDatabase.Slot.Session, SceneDatabase.SceneName.MainMenuScene, setActive: true)
            .Unload(SceneDatabase.Slot.Milestone)
            .ClearAssets()
            .Perform();

        _fsm?.SwitchState(_mainMenuState);
    }

    public void SwitchToGamePlayState()
    {
        // Load Game Scene, if not already loaded
        if (!SceneManager.GetSceneByName(SceneDatabase.SceneName.GameplayScene.ToString()).isLoaded)
            _scenesController.NewTransitionPlan()
                .Load(SceneDatabase.Slot.Session, SceneDatabase.SceneName.GameplayScene, setActive: true)
                .Load(SceneDatabase.Slot.Milestone, SceneDatabase.SceneName.Milestone) // TODO: load restored milestone from local memory
                .WithOverlay()
                .ClearAssets()
                .Perform();

        _fsm?.SwitchState(_gamePlayState);
    }

    public void SwitchToPauseState() => _fsm?.SwitchState(_pauseState);
    #endregion
}
