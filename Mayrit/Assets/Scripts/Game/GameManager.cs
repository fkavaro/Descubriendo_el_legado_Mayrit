using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{

    #region PUBLIC PROPERTIES
    // Finite State Machine
    public FiniteStateMachine<GameManager> _fsm;
    public MainMenu_GameState _mainMenuState;
    public GamePlay_GameState _gamePlayState;
    public Pause_GameState _pauseState;

    public GameInputActions _inputActions;
    public PlayableCharacter _currentPlayableCharacter;
    #endregion

    #region PRIVATE PROPERTIES

    #endregion

    #region INHERITED
    protected override void OnAwake()
    {
        // Singleton
        base.OnAwake();

        _inputActions = new();
        //_inputActions?.Enable();
    }

    protected override void OnStart()
    {

    }

    protected override void OnUpdate()
    {

    }
    protected override ADecisionSystem<GameManager> CreateDecisionSystem()
    {
        _fsm = new(this);

        _mainMenuState = new(_fsm);
        _gamePlayState = new(_fsm);
        _pauseState = new(_fsm);

        // Set initial state based on scene name
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "GameScene")
        {
            _fsm.SetInitialState(_gamePlayState);
        }
        else
        {
            _fsm.SetInitialState(_mainMenuState);
        }

        return _fsm;
    }

    private void OnDestroy()
    {
        _inputActions?.Disable(); // Disables all action maps. To avoi errors
    }

    public PlayableCharacter GetCurrentPlayableCharacter()
    {
        // Find the player character
        _currentPlayableCharacter = FindFirstObjectByType<PlayableCharacter>();

        return _currentPlayableCharacter;
    }
    #endregion

    #region PUBLIC METHODS

    #endregion

    #region PRIVATE METHODS

    #endregion
}
