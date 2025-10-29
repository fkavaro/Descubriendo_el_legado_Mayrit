using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(FiniteStateMachine))]

/// <summary>
/// Manages the game states and data. Singleton.
/// </summary>
public class GameManager : ASingletonBehaviourControllable<GameManager>
{
    #region EDITOR PROPERTIES
    [Header("Player")]
    public PlayableCharacter _currentPlayableCharacter;
    #endregion

    #region PROPERTIES
    [HideInInspector] public FiniteStateMachine _fsm;
    public MainMenu_GameState _mainMenuState;
    public GamePlay_GameState _gamePlayState;
    public Pause_GameState _pauseState;

    public GameInputActions _inputActions;
    #endregion

    #region INHERITED
    protected override void Awake()
    {
        base.Awake(); // Singleton
        _inputActions = new();

        // Subscribe to milestone change event
        ProgressManager.Instance.OnMilestoneChanged += UpdatePlayableCharacter;

        // Find the playable character
        _currentPlayableCharacter = FindFirstObjectByType<PlayableCharacter>();
    }

    public override void SetDecisionSystem()
    {
        // FINITE STATE MACHINE
        _fsm = GetComponent<FiniteStateMachine>();

        _mainMenuState = new(_fsm);
        _gamePlayState = new(_fsm);
        _pauseState = new(_fsm);

        // Set initial state based on scene name
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "GameScene")
            _fsm.SetInitialState(_gamePlayState);
        else
            _fsm.SetInitialState(_mainMenuState);

        _fsm.enabled = true; // Ensure FSM is enabled
    }
    #endregion

    #region MONOBEHAVIOUR
    private void OnDestroy()
    {
        _inputActions?.Disable(); // Disables all action maps. To avoid errors
    }
    #endregion

    void UpdatePlayableCharacter(ProgressManager.Milestone milestone)
    {
        // Find the playable character
        _currentPlayableCharacter = FindFirstObjectByType<PlayableCharacter>();
    }
}
