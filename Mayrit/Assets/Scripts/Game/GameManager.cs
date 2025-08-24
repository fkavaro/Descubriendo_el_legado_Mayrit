using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    #region PUBLIC PROPERTIES
    ABehaviourController<GameManager> _behaviourController;

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
    protected override void Awake()
    {
        // Singleton
        base.Awake();

        _inputActions = new();

        _behaviourController = new(name);

        _fsm = new(_behaviourController);

        _mainMenuState = new(_fsm);
        _gamePlayState = new(_fsm);
        _pauseState = new(_fsm);

        // Set initial state based on scene name
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "GameScene")
            _fsm.SetInitialState(_gamePlayState);
        else
            _fsm.SetInitialState(_mainMenuState);

        _behaviourController.Awake();
    }

    void Start()
    {
        _behaviourController.Start();
    }

    void Update()
    {
        _behaviourController.Update();
    }

    void LateUpdate()
    {
        _behaviourController.LateUpdate();
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
