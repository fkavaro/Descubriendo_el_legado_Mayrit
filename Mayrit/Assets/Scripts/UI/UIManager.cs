using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(UIDocument))]
[RequireComponent(typeof(StackFiniteStateMachine))]

/// <summary>
/// Manages the user interface states and data. Singleton.
/// </summary>
public class UIManager : ASingletonBehaviourControllable<UIManager>
{
    #region EDITOR PROPERTIES
    [Header("User Interface Document")]
    public UIDocument _UIDocument;

    [Header("Tooltip Settings")]
    public Vector2 _tooltipOffset = new(-30, -30);
    #endregion

    #region PROPERTIES
    [HideInInspector] public StackFiniteStateMachine _fsm;
    public MainMenu_UIState _mainMenuState;
    public SpectatorHUD_UIState _spectatorHUDState;
    public PlayerHUD_UIState _playerHUDState;
    public PauseMenu_UIState _pauseState;
    public HeritageMenu_UIState _heritageState;
    #endregion

    #region INHERITED
    public override void SetDecisionSystem()
    {
        // FINITE STATE MACHINE
        _fsm = GetComponent<StackFiniteStateMachine>();
        _fsm.enabled = true; // Ensure FSM is enabled

        _mainMenuState = new(_fsm, GetComponent<UIDocument>());
        _spectatorHUDState = new(_fsm, GetComponent<UIDocument>());
        _playerHUDState = new(_fsm, GetComponent<UIDocument>());
        _pauseState = new(_fsm, GetComponent<UIDocument>());
        _heritageState = new(_fsm, GetComponent<UIDocument>());

        // Set initial state based on scene name
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "GameScene")
            _fsm.SetInitialState(_spectatorHUDState);
        else
            _fsm.SetInitialState(_mainMenuState);

        _fsm.enabled = true; // Ensure FSM is enabled
    }
    #endregion
}
