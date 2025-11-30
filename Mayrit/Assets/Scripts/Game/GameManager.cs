using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the game states and data. Singleton.
/// </summary>
public class GameManager : ASingletonBehaviourEntity<GameManager, FiniteStateMachine<AGameState>>
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
    public event Action<bool> GamePausedEvent;

    GameInputActions _inputActions;
    FiniteStateMachine<AGameState> _fsm;
    MainMenu_GameState _mainMenuState;
    GamePlay_GameState _gamePlayState;
    Pause_GameState _pauseState;
    #endregion

    #region INHERITED
    public override FiniteStateMachine<AGameState> InitializeBehaviourSystem()
    {
        _fsm = new(this);

        // States initialization
        _mainMenuState = new();
        _gamePlayState = new();
        _pauseState = new();

        // Set initial state based on scene name
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "GameScene")
            _fsm.SetInitialState(_gamePlayState);
        else
            _fsm.SetInitialState(_mainMenuState);

        return _fsm;
    }
    #endregion

    #region LIFE CYCLE
    protected override void Awake()
    {
        base.Awake();

        _inputActions = new();


        // Subscribe events
        _pauseState.GamePausedEvent += OnGamePaused;
        ProgressManager.Instance.OnMilestoneChangedEvent += OnMilestoneChanged;
    }

    void OnDestroy()
    {
        _inputActions?.Disable(); // Disables all action maps. To avoid errors
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

    #region EVENT METHODS
    void OnGamePaused(bool isPaused)
    {
        GamePausedEvent?.Invoke(isPaused);
    }

    void OnMilestoneChanged(MilestoneMapping milestoneMapping)
    {
        _playableCharacter = milestoneMapping.PlayableCharacter;
    }
    #endregion
}
