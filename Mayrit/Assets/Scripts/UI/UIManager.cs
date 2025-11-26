using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the user interface states and data. Singleton.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class UIManager : ASingletonBehaviourEntity<UIManager, StackFiniteStateMachine<AUIState>>
{
    #region PROPERTY HELPERS
    public bool IsInMainMenuState => _sfsm.IsCurrentState(_mainMenuState);
    public bool IsInSpectatorHUDState => _sfsm.IsCurrentState(_spectatorHUDState);
    public bool IsInPlayerHUDState => _sfsm.IsCurrentState(_playerHUDState);
    public bool IsInPauseState => _sfsm.IsCurrentState(_pauseState);
    public bool IsInHeritageState => _sfsm.IsCurrentState(_heritageState);
    #endregion

    #region EDITOR PROPERTIES
    [Header("User Interface Document")]
    public UIDocument _UIDocument;

    [Header("Tooltip Settings")]
    public Vector2 _tooltipOffset = new(-30, -30);
    #endregion

    #region INTERNAL PROPERTIES
    public event Action<AInformationSO> ShowContextualPanelEvent;
    public event Action HideContextualPanelEvent;
    public event Action<AInformationSO> ShowTooltipEvent;
    public event Action HideTooltipEvent;
    public event Action ModernSuperpositionToggledEvent;

    StackFiniteStateMachine<AUIState> _sfsm;
    MainMenu_UIState _mainMenuState;
    SpectatorHUD_UIState _spectatorHUDState;
    PlayerHUD_UIState _playerHUDState;
    PauseMenu_UIState _pauseState;
    HeritageMenu_UIState _heritageState;
    #endregion

    #region INHERITED
    public override StackFiniteStateMachine<AUIState> InitializeBehaviourSystem()
    {
        _sfsm = new(this);

        UIDocument uiDocument = GetComponent<UIDocument>();

        // States initialization
        _mainMenuState = new(uiDocument);
        _spectatorHUDState = new(uiDocument);
        _playerHUDState = new(uiDocument);
        _pauseState = new(uiDocument);
        _heritageState = new(uiDocument);

        // Set initial state based on scene name
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "GameScene")
            _sfsm.SetInitialState(_spectatorHUDState);
        else
            _sfsm.SetInitialState(_mainMenuState);

        // Subscribe to events
        _spectatorHUDState.OnModernSuperpositionEvent += OnModernSuperpositionToggled;
        TourManager.Instance.OnTourPOIVisitedEvent += OnTourPOIVisited;

        return _sfsm;
    }
    #endregion

    #region STATE HANDLING
    public void SwitchToMainMenuState()
    {
        _sfsm?.SwitchState(_mainMenuState);
    }

    public void SwitchToSpectatorHUDState()
    {
        _sfsm?.SwitchState(_spectatorHUDState);
    }

    public void SwitchToPlayerHUDState()
    {
        _sfsm?.SwitchState(_playerHUDState);
    }

    public void SwitchToPauseState()
    {
        _sfsm?.SwitchState(_pauseState);
    }

    public void SwitchToHeritageState()
    {
        _sfsm?.SwitchState(_heritageState);
    }
    #endregion

    #region PUBLIC METHODS
    public bool IsCursorOverUI()
    {
        return BehaviourSystem.CurrentState.IsCursorOverUI();
    }

    public void HideContextualPanel()
    {
        HideContextualPanelEvent?.Invoke();
    }

    public void ShowContextualPanel(AInformationSO data)
    {
        ShowContextualPanelEvent?.Invoke(data);
    }

    public void ShowTooltip(AInformationSO data)
    {
        ShowTooltipEvent?.Invoke(data);
    }

    public void HideTooltip()
    {
        HideTooltipEvent?.Invoke();
    }
    #endregion

    #region EVENT METHODS
    void OnModernSuperpositionToggled()
    {
        ModernSuperpositionToggledEvent?.Invoke();
    }

    void OnTourPOIVisited(PointOfInterest poi)
    {
        ShowContextualPanel(poi.Data);
    }
    #endregion
}
