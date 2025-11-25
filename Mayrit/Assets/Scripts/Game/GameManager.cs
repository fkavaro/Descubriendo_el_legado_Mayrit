using System;
using UnityEngine;
using UnityEngine.SceneManagement;


/// <summary>
/// Manages the game states and data. Singleton.
/// </summary>
public class GameManager : ASingletonBehaviourEntity<GameManager, FiniteStateMachine<AGameState>>
{
    #region EDITOR PROPERTIES
    [Header("Player")]
    public PlayableCharacter _playableCharacter; // TODO: make private
    #endregion

    #region INTERNAL PROPERTIES
    public event Action<PlayableCharacter> OnPlayableCharacterChanged;

    FiniteStateMachine<AGameState> _fsm;
    public MainMenu_GameState _mainMenuState;
    public GamePlay_GameState _gamePlayState;
    public Pause_GameState _pauseState;
    public GameInputActions _inputActions;
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

    #region MONOBEHAVIOUR
    protected override void Awake()
    {
        base.Awake();

        _inputActions = new();

        // Subscribe to milestone change event
        ProgressManager.Instance.OnMilestoneChangedEvent += UpdatePlayableCharacter;

        // Find the playable character
        _playableCharacter = FindFirstObjectByType<PlayableCharacter>();
    }

    private void OnDestroy()
    {
        _inputActions?.Disable(); // Disables all action maps. To avoid errors
    }
    #endregion

    #region PRIVATE METHODS
    void UpdatePlayableCharacter(MilestoneMapping milestoneMapping)
    {
        // Find the playable character
        _playableCharacter = FindFirstObjectByType<PlayableCharacter>();
        OnPlayableCharacterChanged?.Invoke(_playableCharacter);
    }
    #endregion
}
