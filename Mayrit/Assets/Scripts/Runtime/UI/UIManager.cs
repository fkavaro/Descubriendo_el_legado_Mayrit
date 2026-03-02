using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class UIManager : ABehaviourEntity<StackFiniteStateMachine<AUIState>>
{
    #region PROPERTY HELPERS
    public bool IsInMainMenuState => _sfsm.IsCurrentState(_mainMenuState);
    public bool IsInSpectatorHUDState => _sfsm.IsCurrentState(_spectatorHUDState);
    public bool IsInPlayerHUDState => _sfsm.IsCurrentState(_playerHUDState);
    public bool IsInPauseState => _sfsm.IsCurrentState(_pauseState);
    public bool IsInSettingsMenuState => _sfsm.IsCurrentState(_settingsMenuState);
    public bool IsInLoadingScreenState => _sfsm.IsCurrentState(_loadingScreenState);
    public bool EdgeScrollingValueSet => _edgeScrollingValueSet;
    public bool ControlsVisibilityValueSet => _controlsVisibilityValueSet;
    public float MusicVolumeValueSet => _musicVolumeValueSet;
    public float SFXVolumeValueSet => _sfxVolumeValueSet;
    public bool IsCursorOverUI => BehaviourSystem.CurrentState.IsCursorOverUI();
    public Vector2 PlayerFollowerScreenMargin => _playerFollowerScreenMargin;
    public Vector2 PlayerFollowerPositionOffset => _playerFollowerPositionOffset;
    #endregion

    #region EDITOR PROPERTIES
    [Header("Fade animation")]
    [SerializeField] float _fadeInDuration = 1f;
    [SerializeField] float _fadeOutDuration = 1f;

    [Header("Player Follower")]
    [SerializeField] Vector2 _playerFollowerScreenMargin = new(100f, 100f);
    [SerializeField] Vector2 _playerFollowerPositionOffset = new(25f, 25f);

    [Header("Settings menu values")]
    [SerializeField] bool _edgeScrollingValueSet = true;
    [SerializeField] bool _controlsVisibilityValueSet = true;
    [SerializeField] float _musicVolumeValueSet = 1f;
    [SerializeField] float _sfxVolumeValueSet = 1f;
    #endregion

    #region INTERNAL PROPERTIES
    UIDocument _uiDocument;

    // Events
    public event Action<DataSO, bool> ContextualPanelShownEvent;
    public event Action ContextualPanelHiddenEvent;

    public event Action PlayTourClickedEvent;
    public event Action ResetTourClickedEvent;
    public event Action<bool> EdgeScrollingToggledEvent;
    public event Action<float> MusicVolumeChangedEvent;
    public event Action<float> SFXVolumeChangedEvent;
    public event Action<bool> ModernVisualizationToggled;
    public event Action<bool> LandmarkVisualizationToggled;
    public bool IsModernVisualizationOn { get => _spectatorHUDState._modernVisualizactionSwitch.Value; }
    public bool IsLandmarkVisualizationOn { get => _spectatorHUDState._landmarkVisualizationSwitch.Value; }

    // Stack FSM
    StackFiniteStateMachine<AUIState> _sfsm;
    MainMenu_UIState _mainMenuState;
    SpectatorHUD_UIState _spectatorHUDState;
    PlayerHUD_UIState _playerHUDState;
    PauseMenu_UIState _pauseState;
    SettingsMenu_UIState _settingsMenuState;
    LoadingScreen_UIState _loadingScreenState;

    // Dependency Injection
    ScenesController _scenesController;
    TourManager _tourManager;
    CameraManager _cameraManager;
    #endregion

    #region INHERITED
    public override StackFiniteStateMachine<AUIState> DefineBehaviourSystemOnAwake()
    {
        _sfsm = new(this);

        _uiDocument = GetComponent<UIDocument>();

        // States initialization
        _mainMenuState = new(_uiDocument, _fadeInDuration, _fadeOutDuration);
        _spectatorHUDState = new(_uiDocument, _fadeInDuration * 5f, _fadeOutDuration);
        _playerHUDState = new(_uiDocument, _fadeInDuration, _fadeOutDuration);
        _pauseState = new(_uiDocument, _fadeInDuration, _fadeOutDuration);
        _settingsMenuState = new(_uiDocument, 0f, 0f);
        _loadingScreenState = new(_uiDocument, _fadeInDuration, _fadeOutDuration);

        // State AwakeState calls
        _mainMenuState.AwakeState();
        _spectatorHUDState.AwakeState();
        _playerHUDState.AwakeState();
        _pauseState.AwakeState();
        _settingsMenuState.AwakeState();
        _loadingScreenState.AwakeState();

        _sfsm.SetInitialState(_mainMenuState);

        return _sfsm;
    }
    #endregion

    #region LIFE CYCLE
    protected override void Awake()
    {
        base.Awake();

        ServiceLocator.Instance.Register(this);
    }

    protected override void Start()
    {
        _scenesController = ServiceLocator.Instance.Get<ScenesController>();
        _scenesController.ScenesLoadedFullyEvent += OnScenesLoadedFully;
        _scenesController.SceneLoadedPartiallyEvent += OnSceneLoadedPartially;

        //base.Start(); When main menu scene is loaded
    }

    void OnDisable()
    {
        _scenesController.ScenesLoadedFullyEvent -= OnScenesLoadedFully;
        _scenesController.SceneLoadedPartiallyEvent -= OnSceneLoadedPartially;
        ServiceLocator.Instance.Unregister(this);
    }
    #endregion

    #region STATE HANDLING
    public void SwitchToMainMenuState()
    {
        _sfsm?.SwitchState(_mainMenuState);
    }
    public void SwitchToSpectatorHUDState() => _sfsm?.SwitchState(_spectatorHUDState);
    public void SwitchToPlayerHUDState() => _sfsm?.SwitchState(_playerHUDState);
    public void SwitchToPauseState() => _sfsm?.SwitchState(_pauseState);
    public void SwitchToSettingsMenuState() => _sfsm?.SwitchState(_settingsMenuState);
    public void SwitchToLoadingScreenState() => _sfsm?.SwitchState(_loadingScreenState);
    #endregion

    #region PUBLIC METHODS
    public void ShowContextualPanel(DataSO data, bool isCharacterData = false)
    {
        ContextualPanelShownEvent?.Invoke(data, isCharacterData);
    }

    public void HideContextualPanel()
    {
        //HideContextualPanelEvent?.Invoke();
        ContextualPanelHiddenEvent?.Invoke();
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
    #endregion

    #region CALLBACK METHODS
    void OnSceneLoadedPartially(SceneDatabase.SceneType type, SceneDatabase.SceneName name)
    {
        // If main menu loaded, switches to initial state: main menu stat
        if (name == SceneDatabase.SceneName.MainMenuScene)
            // Only background will be shown, buttons will be shown when the scene is fully loaded
            base.Start();
    }

    void OnScenesLoadedFully(Dictionary<SceneDatabase.SceneType, SceneDatabase.SceneName> loadedScenes, List<SceneDatabase.SceneType> unloadedTypes)
    {
        if (loadedScenes.ContainsValue(SceneDatabase.SceneName.MainMenuScene))
        {
            _playerHUDState.ContextualPanelHiddenEvent -= OnContextualPanelHidden;
            _spectatorHUDState.ContextualPanelHiddenEvent -= OnContextualPanelHidden;
            _spectatorHUDState.PlayTourEvent -= OnPlayTourClicked;
            _spectatorHUDState.ResetTourEvent -= OnResetTourClicked;
            _spectatorHUDState._modernVisualizactionSwitch.Toggled -= OnModernVisualizationToggled;
            _spectatorHUDState._landmarkVisualizationSwitch.Toggled -= OnLandmarkVisualizationToggled;

            if (_tourManager != null)
            {
                _tourManager.POIVisitedEvent -= OnTourPOIVisited;
                _tourManager = null;
            }

            if (_cameraManager != null)
            {
                _cameraManager.CameraStateChangedEvent -= OnCameraStateChanged;
                _cameraManager = null;
            }
        }
        // In gameplay scene
        else if (loadedScenes.ContainsValue(SceneDatabase.SceneName.GameplayScene))
        {
            // Get dependencies from ServiceLocator
            _tourManager = ServiceLocator.Instance.Get<TourManager>();
            _cameraManager = ServiceLocator.Instance.Get<CameraManager>();

            // Subscribe to events
            _playerHUDState.ContextualPanelHiddenEvent += OnContextualPanelHidden;
            _spectatorHUDState.ContextualPanelHiddenEvent += OnContextualPanelHidden;
            _spectatorHUDState.PlayTourEvent += OnPlayTourClicked;
            _spectatorHUDState.ResetTourEvent += OnResetTourClicked;
            _spectatorHUDState._modernVisualizactionSwitch.Toggled += OnModernVisualizationToggled;
            _spectatorHUDState._landmarkVisualizationSwitch.Toggled += OnLandmarkVisualizationToggled;
            _tourManager.POIVisitedEvent += OnTourPOIVisited;
            _cameraManager.CameraStateChangedEvent += OnCameraStateChanged;
        }

        // A milestone scene loaded
        if (loadedScenes.TryGetValue(SceneDatabase.SceneType.Milestone, out var milestoneScene))
            StartCoroutine(FadeInSpecatorHUDCoroutine()); // TODO: Improve this
    }

    private void OnCameraStateChanged()
    {
        if (_cameraManager.IsInSpectatorState || _cameraManager.IsInOrbitalState)
            SwitchToSpectatorHUDState();
        else
            SwitchToPlayerHUDState();
    }

    void OnPlayTourClicked()
    {
        _playerHUDState._showTourEnd = _tourManager.CurrentTour != null && _tourManager.CurrentTour.IsCompleted;
        SwitchToPlayerHUDState();
        PlayTourClickedEvent?.Invoke();
    }

    void OnResetTourClicked()
    {
        _playerHUDState._showTourEnd = false;
        StartCoroutine(ResetTourWithBlackFadeCoroutine());
    }

    IEnumerator ResetTourWithBlackFadeCoroutine()
    {
        yield return FadeInBlackLoadingScreenCoroutine();
        ResetTourClickedEvent?.Invoke();
        yield return new WaitForSeconds(1f);
        yield return FadeOutBlackLoadingScreenCoroutine();
        SwitchToPlayerHUDState();
    }

    void OnModernVisualizationToggled(bool value)
    {
        ModernVisualizationToggled?.Invoke(value);
    }

    void OnLandmarkVisualizationToggled(bool value)
    {
        LandmarkVisualizationToggled?.Invoke(value);
    }

    void OnTourPOIVisited(PointOfInterest poi)
    {
        ShowContextualPanel(poi.Data);
    }

    void OnContextualPanelHidden()
    {
        ContextualPanelHiddenEvent?.Invoke();
    }
    #endregion

    #region COROUTINES
    public IEnumerator FadeInLoadingScreenCoroutine()
    {
        SwitchToLoadingScreenState();
        yield return _loadingScreenState.FadeInCoroutine();
    }

    public IEnumerator FadeOutLoadingScreenCoroutine()
    {
        // Fade out after continue button is clicked in loading screen
        while (!_loadingScreenState.ContinueIsClicked)
            yield return null;
        yield return _loadingScreenState.FadeOutCoroutine();
    }

    public IEnumerator FadeInBlackLoadingScreenCoroutine()
    {
        SwitchToLoadingScreenState();
        yield return _loadingScreenState.BlackFadeInCoroutine();
    }

    public IEnumerator FadeOutBlackLoadingScreenCoroutine()
    {
        yield return _loadingScreenState.BlackFadeOutCoroutine();
    }

    IEnumerator FadeInSpecatorHUDCoroutine()
    {
        SwitchToSpectatorHUDState();
        yield return _spectatorHUDState.FadeInCoroutine();
    }
    #endregion
}
