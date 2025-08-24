using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>, IBehaviourControllable
{
    #region PUBLIC PROPERTIES
    [Header("Behaviour Controller Properties")]
    [Tooltip("Whether to show debug messages in the console or not")]
    [SerializeField] bool _debugMode = false;
    [Tooltip("Whether to update next frame or not")]
    [SerializeField] bool _isExecutionPaused = false;

    public string Name => gameObject.name;
    public bool DebugMode
    {
        get => _debugMode;
        set => _debugMode = value;
    }
    public bool IsExecutionPaused
    {
        get => _isExecutionPaused;
        set => _isExecutionPaused = value;
    }

    [Header("Player")]
    public PlayableCharacter _currentPlayableCharacter;

    public ABehaviourController _behaviourController;

    // Finite State Machine
    public FiniteStateMachine _fsm;
    public MainMenu_GameState _mainMenuState;
    public GamePlay_GameState _gamePlayState;
    public Pause_GameState _pauseState;

    public GameInputActions _inputActions;
    #endregion

    #region PRIVATE PROPERTIES

    #endregion

    #region INHERITED
    protected override void Awake()
    {
        // Singleton
        base.Awake();

        _inputActions = new();

        _behaviourController = new(this);

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
        _inputActions?.Disable(); // Disables all action maps. To avoid errors
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
