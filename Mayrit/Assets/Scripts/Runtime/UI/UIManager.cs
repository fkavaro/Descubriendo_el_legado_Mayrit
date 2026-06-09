using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class UIManager : ABehaviourEntity<StackFiniteStateMachine<AUIState>>
{
    #region PROPERTY HELPERS
    public UIDocument UIDocument => _uiDocument;
    public bool IsInMainMenuState => _sfsm.IsCurrentState(_mainMenuState);
    public bool IsInAerialHUDState => _sfsm.IsCurrentState(_aerialHUDState);
    public bool IsInPlayerHUDState => _sfsm.IsCurrentState(_playerHUDState);
    public bool IsInPauseState => _sfsm.IsCurrentState(_pauseState);
    public bool IsInInformationDisplayState => _sfsm.IsCurrentState(_informationDisplayState);
    public InformationDisplay_UIState ContextualPanelState => _informationDisplayState;
    public bool IsInSettingsMenuState => _sfsm.IsCurrentState(_settingsMenuState);
    public bool IsInLoadingScreenState => _sfsm.IsCurrentState(_loadingScreenState);
    public bool EdgeScrollingValueSet => _edgeScrollingValueSet;
    public bool POIsVisibilityValueSet => _POIsVisibilityValueSet;
    public bool ControlsVisibilityValueSet => _controlsVisibilityValueSet;
    public bool ModernVisualizationValueSet => _modernVisualizationValueSet;
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
    [SerializeField] bool _POIsVisibilityValueSet = true;
    [SerializeField] bool _controlsVisibilityValueSet = true;
    [SerializeField] bool _modernVisualizationValueSet = false;
    [SerializeField] float _musicVolumeValueSet = 1f;
    [SerializeField] float _sfxVolumeValueSet = 1f;
    #endregion

    #region INTERNAL PROPERTIES
    UIDocument _uiDocument;
    ContextualPanelComponent _contextualPanelComponent;

    // Events
    public event Action StateChangedEvent;
    public event Action PlayTourClickedEvent;
    public event Action ResetTourClickedEvent;
    public event Action<bool> EdgeScrollingToggledEvent;
    public event Action<float> MusicVolumeChangedEvent;
    public event Action<float> SFXVolumeChangedEvent;
    public event Action<bool> ModernVisualizationToggled;
    public event Action<bool> POIsVisualizationToggledEvent;

    // Stack FSM
    StackFiniteStateMachine<AUIState> _sfsm;
    MainMenu_UIState _mainMenuState;
    AerialHUD_UIState _aerialHUDState;
    PlayerHUD_UIState _playerHUDState;
    PauseMenu_UIState _pauseState;
    InformationDisplay_UIState _informationDisplayState;
    SettingsMenu_UIState _settingsMenuState;
    LoadingScreen_UIState _loadingScreenState;
    CreditsScreen_UIState _creditsScreenState;

    // Dependency Injection
    ScenesController _scenesController;
    CameraManager _cameraManager;
    ProgressManager _progressManager;
    #endregion

    #region INHERITED
    public override StackFiniteStateMachine<AUIState> DefineBehaviourSystem()
    {
        _sfsm = new(this);

        _uiDocument = GetComponent<UIDocument>();

        _contextualPanelComponent = new(_uiDocument, _fadeInDuration, _fadeOutDuration);
        _contextualPanelComponent.AwakeState();

        // States initialization
        _mainMenuState = new(_uiDocument, _fadeInDuration, _fadeOutDuration);
        _aerialHUDState = new(_uiDocument, _fadeInDuration * 5f, _fadeOutDuration);
        _playerHUDState = new(_uiDocument, _fadeInDuration, _fadeOutDuration);
        _pauseState = new(_uiDocument, _fadeInDuration, _fadeOutDuration);
        _informationDisplayState = new(_uiDocument, _fadeInDuration, _fadeOutDuration, _contextualPanelComponent);
        _settingsMenuState = new(_uiDocument, 0f, 0f);
        _loadingScreenState = new(_uiDocument, _fadeInDuration, _fadeOutDuration, _contextualPanelComponent);
        _creditsScreenState = new(_uiDocument, 0f, 0f);

        // State AwakeState calls
        _mainMenuState.AwakeState();
        _aerialHUDState.AwakeState();
        _playerHUDState.AwakeState();
        _pauseState.AwakeState();
        _informationDisplayState.AwakeState();
        _settingsMenuState.AwakeState();
        _loadingScreenState.AwakeState();
        _creditsScreenState.AwakeState();

        _sfsm.SwitchedStateEvent += OnSwitchedState;
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
    public void SwitchToAerialHUDState() => _sfsm?.SwitchState(_aerialHUDState);
    public void SwitchToPlayerHUDState() => _sfsm?.SwitchState(_playerHUDState);
    public void SwitchToPauseState() => _sfsm?.SwitchState(_pauseState);
    public void SwitchToInformationDisplayState() => _sfsm?.SwitchState(_informationDisplayState);
    public void SwitchToInformationDisplayState(DataSO data)
    {
        _informationDisplayState.DataToShow = data;
        _sfsm?.SwitchState(_informationDisplayState);
    }
    public void SwitchToSettingsMenuState() => _sfsm?.SwitchState(_settingsMenuState);
    public void SwitchToLoadingScreenState() => _sfsm?.SwitchState(_loadingScreenState);
    public void SwitchToCreditsScreenState() => _sfsm?.SwitchState(_creditsScreenState);
    #endregion

    #region PUBLIC METHODS
    public void SetEdgeScrollingValue(bool newValue)
    {
        _edgeScrollingValueSet = newValue;
        EdgeScrollingToggledEvent?.Invoke(newValue);
    }

    public void SetPOIsVisibility(bool newValue)
    {
        _POIsVisibilityValueSet = newValue;
        POIsVisualizationToggledEvent?.Invoke(newValue);
    }

    public void SetControlsVisibility(bool newValue)
    {
        _controlsVisibilityValueSet = newValue;
    }

    public void SetModernVisualizationValue(bool newValue)
    {
        _modernVisualizationValueSet = newValue;
        ModernVisualizationToggled?.Invoke(newValue);
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
    void OnSwitchedState()
    {
        StateChangedEvent?.Invoke();
    }

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
            _informationDisplayState.ClosedEvent -= OnContextualPanelHidden;
            _informationDisplayState.PlayTourClickedEvent -= OnPlayTourClicked;
            _informationDisplayState.ResetTourClickedEvent -= OnResetTourClicked;
            _aerialHUDState._modernVisualizactionSwitch.Toggled -= OnModernVisualizationToggled;
            _aerialHUDState.MilestoneInfoClickedEvent -= OnMilestoneInfoClicked;

            if (_cameraManager != null)
            {
                _cameraManager.CameraStateChangedEvent -= OnCameraStateChanged;
                _cameraManager = null;
            }

            if (_progressManager != null)
            {
                _progressManager = null;
            }
        }
        // In gameplay scene
        else if (loadedScenes.ContainsValue(SceneDatabase.SceneName.GameplayScene))
        {
            // Get dependencies from ServiceLocator
            _cameraManager = ServiceLocator.Instance.Get<CameraManager>();
            _progressManager = ServiceLocator.Instance.Get<ProgressManager>();

            // Subscribe to events
            _informationDisplayState.ClosedEvent += OnContextualPanelHidden;
            _informationDisplayState.PlayTourClickedEvent += OnPlayTourClicked;
            _informationDisplayState.ResetTourClickedEvent += OnResetTourClicked;
            _aerialHUDState._modernVisualizactionSwitch.Toggled += OnModernVisualizationToggled;
            _aerialHUDState.MilestoneInfoClickedEvent += OnMilestoneInfoClicked;
            _cameraManager.CameraStateChangedEvent += OnCameraStateChanged;
        }

        // A milestone scene loaded
        if (loadedScenes.TryGetValue(SceneDatabase.SceneType.Milestone, out var milestoneScene))
            SwitchToAerialHUDState();
    }

    private void OnCameraStateChanged()
    {
        if (_cameraManager.IsInAerialState)
            SwitchToAerialHUDState();
        else if (_cameraManager.IsInOrbitalState)
            SwitchToInformationDisplayState(_cameraManager.OrbitalState.Setting.DataToShow);
        else if (_cameraManager.IsInTourStopState)
            SwitchToInformationDisplayState(_cameraManager.TourStopState.DataToShow);
        else if (_cameraManager.IsInThirdPersonState)
            SwitchToPlayerHUDState();
    }

    void OnPlayTourClicked()
    {
        PlayTourClickedEvent?.Invoke();
        SwitchToPlayerHUDState();
    }

    void OnResetTourClicked()
    {
        StartCoroutine(ResetTourWithBlackFadeCoroutine());
    }

    IEnumerator ResetTourWithBlackFadeCoroutine()
    {
        yield return FadeInBlackLoadingScreenCoroutine();
        _playerHUDState.ShownCompletedTourVisual = false;
        ResetTourClickedEvent?.Invoke();
        yield return new WaitForSeconds(_fadeInDuration);
        yield return FadeOutBlackLoadingScreenCoroutine();
        SwitchToPlayerHUDState();
    }

    void OnModernVisualizationToggled(bool value)
    {
        ModernVisualizationToggled?.Invoke(value);
    }

    void OnMilestoneInfoClicked()
    {
        SwitchToInformationDisplayState(_progressManager.CurrentMilestoneData);
    }

    void OnContextualPanelHidden()
    {
        AUIState previousState = _sfsm?.PopPreviousState();

        if (previousState != _pauseState)
            _sfsm.SwitchState(previousState);
        else
            if (_cameraManager.IsInOrbitalState)
                if (_cameraManager.OrbitalState.Setting.TransitionToApply == CameraTransition.AerialCamera)
                    SwitchToAerialHUDState();
                else
                    SwitchToPlayerHUDState();
            else
                SwitchToPlayerHUDState();
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
        while (!_loadingScreenState.IsContinueClicked)
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
        SwitchToAerialHUDState();
        yield return _aerialHUDState.FadeInCoroutine();
    }
    #endregion
}
