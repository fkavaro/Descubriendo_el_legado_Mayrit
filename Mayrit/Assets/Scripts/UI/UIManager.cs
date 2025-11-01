using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the user interface states and data. Singleton.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class UIManager : ASingletonBehaviourEntity<UIManager, StackFiniteStateMachine>
{
    #region EDITOR PROPERTIES
    [Header("User Interface Document")]
    public UIDocument _UIDocument;

    [Header("Tooltip Settings")]
    public Vector2 _tooltipOffset = new(-30, -30);
    #endregion

    #region INTERNAL PROPERTIES
    StackFiniteStateMachine _sfsm;
    public MainMenu_UIState _mainMenuState;
    public SpectatorHUD_UIState _spectatorHUDState;
    public PlayerHUD_UIState _playerHUDState;
    public PauseMenu_UIState _pauseState;
    public HeritageMenu_UIState _heritageState;
    #endregion

    #region INHERITED
    public override StackFiniteStateMachine InitializeBehaviourSystem()
    {
        _sfsm = new(this);

        UIDocument uiDocument = GetComponent<UIDocument>();

        // States initialization
        _mainMenuState = new(_sfsm, uiDocument);
        _spectatorHUDState = new(_sfsm, uiDocument);
        _playerHUDState = new(_sfsm, uiDocument);
        _pauseState = new(_sfsm, uiDocument);
        _heritageState = new(_sfsm, uiDocument);

        // Set initial state based on scene name
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "GameScene")
            _sfsm.SetInitialState(_spectatorHUDState);
        else
            _sfsm.SetInitialState(_mainMenuState);

        return _sfsm;
    }
    #endregion
}
