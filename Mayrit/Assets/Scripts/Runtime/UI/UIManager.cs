using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages the user interface states and data. Singleton.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class UIManager : ABehaviourEntity<StackFiniteStateMachine<AUIState>>
{
    #region PROPERTY HELPERS
    public bool IsInMainMenuState => _sfsm.IsCurrentState(_mainMenuState);
    public bool IsInSpectatorHUDState => _sfsm.IsCurrentState(_spectatorHUDState);
    public bool IsInPlayerHUDState => _sfsm.IsCurrentState(_playerHUDState);
    public bool IsInPauseState => _sfsm.IsCurrentState(_pauseState);
    public bool IsInHeritageState => _sfsm.IsCurrentState(_heritageState);
    public Vector2 TooltipOffset => _tooltipOffset;
    public bool EdgeScrollingValueSet => _edgeScrollingValueSet;
    public bool ControlsVisibilityValueSet => _controlsVisibilityValueSet;
    public float MusicVolumeValueSet => _musicVolumeValueSet;
    public float SFXVolumeValueSet => _sfxVolumeValueSet;
    public bool IsCursorOverUI => BehaviourSystem.CurrentState.IsCursorOverUI();
    #endregion

    #region EDITOR PROPERTIES
    [Header("Loading Screen")]
    [SerializeField] float _fadeInDuration = 1f;
    [SerializeField] float _fadeOutDuration = 1f;

    [Header("Other settings")]
    [SerializeField] Vector2 _tooltipOffset = new(-30, -30);
    [SerializeField] bool _edgeScrollingValueSet = true;
    [SerializeField] bool _controlsVisibilityValueSet = true;
    [SerializeField] float _musicVolumeValueSet = 1f;
    [SerializeField] float _sfxVolumeValueSet = 1f;
    #endregion

    #region INTERNAL PROPERTIES
    UIDocument _uiDocument;

    // Events
    public event Action<DataSO, bool> ShowContextualPanelEvent;
    public event Action OnContextualPanelHiddenEvent;
    public event Action<DataSO> ShowTooltipEvent;
    public event Action HideTooltipEvent;
    public event Action PlayCharacterClickedEvent;
    public event Action ModernSuperpositionToggledEvent;
    public event Action<bool> EdgeScrollingToggledEvent;
    public event Action<float> MusicVolumeChangedEvent;
    public event Action<float> SFXVolumeChangedEvent;

    // Stack FSM
    StackFiniteStateMachine<AUIState> _sfsm;
    MainMenu_UIState _mainMenuState;
    SpectatorHUD_UIState _spectatorHUDState;
    PlayerHUD_UIState _playerHUDState;
    PauseMenu_UIState _pauseState;
    HeritageMenu_UIState _heritageState;
    SettingsMenu_UIState _settingsMenuState;
    LoadingScreen_UIState _loadingScreenState;

    // Dependency Injection
    ScenesController _scenesController;
    TourManager _tourManager;
    #endregion

    #region INHERITED
    public override StackFiniteStateMachine<AUIState> DefineBehaviourSystemOnAwake()
    {
        _sfsm = new(this);

        _uiDocument = GetComponent<UIDocument>();

        // States initialization
        _mainMenuState = new(_uiDocument);
        _spectatorHUDState = new(_uiDocument);
        _playerHUDState = new(_uiDocument);
        _pauseState = new(_uiDocument);
        _heritageState = new(_uiDocument);
        _settingsMenuState = new(_uiDocument);
        _loadingScreenState = new(_uiDocument, _fadeInDuration, _fadeOutDuration);

        // State AwakeState calls
        _mainMenuState.AwakeState();
        _spectatorHUDState.AwakeState();
        _playerHUDState.AwakeState();
        _pauseState.AwakeState();
        _heritageState.AwakeState();
        _settingsMenuState.AwakeState();
        _loadingScreenState.AwakeState();

        _sfsm.SetInitialState(_mainMenuState);

        return _sfsm;
    }
    #endregion

    #region LIFE CYCLE
    protected override void Awake()
    {
        // Only allow the registered UIManager to initialize
        var registered = ServiceLocator.Instance.Get<UIManager>();
        if (registered != null && registered != this)
        {
            Destroy(gameObject);
            return;
        }

        // Register to Service Locator
        ServiceLocator.Instance.Register(this);

        // Get dependencies from ServiceLocator
        _scenesController = ServiceLocator.Instance.Get<ScenesController>();
        _scenesController.SceneChangedEvent += OnSceneChanged;
        _scenesController.ShowLoadScreenEvent += SwitchToLoadingScreenState;

        base.Awake();
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        _scenesController.SceneChangedEvent -= OnSceneChanged;
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

    public void SwitchToSettingsMenuState()
    {
        _sfsm?.SwitchState(_settingsMenuState);
    }

    public void SwitchToLoadingScreenState()
    {
        _sfsm?.SwitchState(_loadingScreenState);
    }
    #endregion

    #region PUBLIC METHODS
    public void ShowContextualPanel(DataSO data, bool isCharacterData = false)
    {
        ShowContextualPanelEvent?.Invoke(data, isCharacterData);
    }

    public void ShowTooltip(DataSO data)
    {
        ShowTooltipEvent?.Invoke(data);
    }

    public void HideTooltip()
    {
        HideTooltipEvent?.Invoke();
    }

    public void SetControlsVisibility(bool newValue)
    {
        _controlsVisibilityValueSet = newValue;
    }

    public void InvokeEdgeScrollingToggledEvent(bool newValue)
    {
        EdgeScrollingToggledEvent?.Invoke(newValue);
        _edgeScrollingValueSet = newValue;
    }

    public void InvokeMusicVolumeChangedEvent(float newValue)
    {
        MusicVolumeChangedEvent?.Invoke(newValue);
    }

    public void InvokeSFXVolumeChangedEvent(float newValue)
    {
        SFXVolumeChangedEvent?.Invoke(newValue);
    }

    // TODO remove
    // public IEnumerator FadeInLoadingScreen()
    // {
    //     SwitchToLoadingScreenState();
    //     yield return _loadingScreenState.FadeInCoroutine();
    // }

    // public IEnumerator FadeOutLoadingScreen()
    // {
    //     yield return _loadingScreenState.FadeOutCoroutine();
    // }
    #endregion

    #region CALLBACK METHODS
    void OnSceneChanged(Dictionary<string, string> loadedScenes, List<string> unloadedSlots)
    {
        if (loadedScenes.ContainsValue(SceneDatabase.Name.GamePlayScene))
        {
            SwitchToSpectatorHUDState();

            // Get dependencies from ServiceLocator
            _tourManager = ServiceLocator.Instance.Get<TourManager>();

            // Subscribe to events
            _playerHUDState.ContextualPanelHiddenEvent += OnContextualPanelHidden;
            _spectatorHUDState.ContextualPanelHiddenEvent += OnContextualPanelHidden;
            _spectatorHUDState.PlayCharacterEvent += OnPlayCharacterClicked;
            _spectatorHUDState.OnModernSuperpositionEvent += OnModernSuperpositionToggled;
            _tourManager.POIVisitedEvent += OnTourPOIVisited;
        }
        else if (unloadedSlots.Contains(SceneDatabase.Slot.Session))
        {
            SwitchToMainMenuState();

            // Unsubscribe from events
            if (_tourManager != null)
                _tourManager.POIVisitedEvent -= OnTourPOIVisited;

            _playerHUDState.ContextualPanelHiddenEvent -= OnContextualPanelHidden;
            _spectatorHUDState.ContextualPanelHiddenEvent -= OnContextualPanelHidden;
            _spectatorHUDState.PlayCharacterEvent -= OnPlayCharacterClicked;
            _spectatorHUDState.OnModernSuperpositionEvent -= OnModernSuperpositionToggled;
        }
    }

    void OnPlayCharacterClicked()
    {
        SwitchToPlayerHUDState();
        PlayCharacterClickedEvent?.Invoke();
    }

    void OnModernSuperpositionToggled()
    {
        ModernSuperpositionToggledEvent?.Invoke();
    }

    void OnTourPOIVisited(PointOfInterest poi)
    {
        ShowContextualPanel(poi.Data);
    }

    void OnContextualPanelHidden()
    {
        OnContextualPanelHiddenEvent?.Invoke();
    }
    #endregion
}
