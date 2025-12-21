using System;
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
    ProgressManager _progressManager;
    SoundManager _soundManager;
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

        // Set initial state based on scene name
        if (SceneManager.GetActiveScene().name == "GameScene")
            _fsm.SetInitialState(_gamePlayState);
        else
            _fsm.SetInitialState(_mainMenuState);

        return _fsm;
    }
    #endregion

    #region LIFE CYCLE
    protected override void Awake()
    {
        // Subscribe to scene change event
        SceneManager.sceneLoaded += OnSceneLoaded;

        _soundManager = ServiceLocator.Instance.Get<SoundManager>();

        _inputActions = new();

        base.Awake();
    }

    void OnDestroy()
    {
        // Unsubscribe from scene change event
        SceneManager.sceneLoaded -= OnSceneLoaded;

        _inputActions = null;
    }
    #endregion

    #region PUBLIC METHODS
    public void SwitchToMainMenuState()
    {
        _fsm.SwitchState(_mainMenuState);
    }

    public void SwitchToGamePlayState()
    {
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
        _playableCharacter = milestoneMapping.PlayableCharacter;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (_soundManager == null)
            _soundManager = ServiceLocator.Instance.Get<SoundManager>();

        // In main menu scene
        if (SceneManager.GetActiveScene().name == "MainMenuScene")
        {
            // Play menu music
            _soundManager.PlayMenuMusic();
            return;
        }
        // In game play scene
        else if (SceneManager.GetActiveScene().name == "GameScene")
        {
            // Play gameplay music
            _soundManager.PlayGameplayMusic();

            // Get dependencies from ServiceLocator
            _progressManager = ServiceLocator.Instance.Get<ProgressManager>();

            // Subscribe to events
            _progressManager.OnMilestoneChangedEvent += OnMilestoneChanged;
        }
    }
    #endregion
}
