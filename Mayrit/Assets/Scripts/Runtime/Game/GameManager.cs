using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameManager : ABehaviourEntity<FiniteStateMachine<AGameState>>
{
    #region PROPERTY HELPERS
    public GameInputActions InputActions => _inputActions;

    public bool IsInMainMenuState => _fsm.IsCurrentState(_mainMenuState);
    public bool IsLoadGameState => _fsm.IsCurrentState(_loadGameState);
    public bool IsInPauseState => _fsm.IsCurrentState(_pauseState);
    public bool IsInAerialState => _fsm.IsCurrentState(_aerialState);
    public bool IsInThirdPersonState => _fsm.IsCurrentState(_thirdPersonState);
    public bool IsAtPOIState => _fsm.IsCurrentState(_atPOIState);
    public AtPOI_GameState AtPOIState => _atPOIState;
    public bool IsAtTourStopState => _fsm.IsCurrentState(_atTourStopState);
    public AtTourStop_GameState TourStopState => _atTourStopState;
    public bool IsAtCollectibleState => _fsm.IsCurrentState(_atCollectibleState);
    public AtCollectible_GameState AtCollectibleState => _atCollectibleState;

    public float SimulationSpeed => _gameSimulationSpeed;
    public bool EdgeScrollingValueSet => _edgeScrollingValueSet;
    public bool POIsVisibilityValueSet => _POIsVisibilityValueSet;
    public bool ControlsVisibilityValueSet => _controlsVisibilityValueSet;
    public bool ModernVisualizationValueSet => _modernVisualizationValueSet;
    public float MusicVolumeValueSet => _musicVolumeValueSet;
    public float SFXVolumeValueSet => _sfxVolumeValueSet;

    public ScenesController ScenesController => _scenesController;
    public UISystem UISystem => _uiSystem;
    public SoundSystem SoundSystem => _soundSystem;
    public CameraSystem CameraSystem => _cameraSystem;
    public PlayableCharacter PlayableCharacter => _playableCharacter;
    public ProgressManager ProgressManager => _progressManager;
    #endregion

    #region EDITOR PROPERTIES
    [Tooltip("Game simulation speed multiplier. Set by Camera states.")]
    [Range(0.1f, 10f)]
    [SerializeField] float _gameSimulationSpeed = 1f;

    [Header("Settings values")]
    [SerializeField] bool _edgeScrollingValueSet = true;
    [SerializeField] bool _POIsVisibilityValueSet = true;
    [SerializeField] bool _controlsVisibilityValueSet = true;
    [SerializeField] bool _modernVisualizationValueSet = false;
    [SerializeField] float _musicVolumeValueSet = 1f;
    [SerializeField] float _sfxVolumeValueSet = 1f;
    #endregion

    #region INTERNAL PROPERTIES
    public event Action StateChangedEvent;

    // UI Events
    public event Action<bool> EdgeScrollingToggledEvent;
    public event Action<float> MusicVolumeChangedEvent;
    public event Action<float> SFXVolumeChangedEvent;
    public event Action<bool> ModernVisualizationToggled;
    public event Action<bool> POIsVisualizationToggledEvent;
    public event Action<bool> ControlsVisibilityToggledEvent;
    public event Action PlayTourClickedEvent;
    public event Action ResetTourClickedEvent;

    // Progress Events
    public event Action<Milestone_DataSO> MilestoneChangedEvent;

    GameInputActions _inputActions;
    FiniteStateMachine<AGameState> _fsm;
    MainMenu_GameState _mainMenuState;
    LoadGame_GameState _loadGameState;
    Pause_GameState _pauseState;
    Aerial_GameState _aerialState;
    ThirdPerson_GameState _thirdPersonState;
    AtPOI_GameState _atPOIState;
    AtTourStop_GameState _atTourStopState;
    AtCollectible_GameState _atCollectibleState;

    ScenesController _scenesController;
    UISystem _uiSystem;
    SoundSystem _soundSystem;
    CameraSystem _cameraSystem;
    PlayableCharacter _playableCharacter;
    ProgressManager _progressManager;
    TourManager _tourManager;
    CollectiblesManager _collectiblesManager;
    #endregion

    #region INHERITED
    public override FiniteStateMachine<AGameState> DefineBehaviourSystem()
    {
        _fsm = new(this);

        // States initialization
        _mainMenuState = new(this);
        _loadGameState = new(this);
        _pauseState = new(this);
        _aerialState = new(this);
        _thirdPersonState = new(this);
        _atPOIState = new(this);
        _atTourStopState = new(this);
        _atCollectibleState = new(this);

        // State AwakeState calls
        _mainMenuState.AwakeState();
        _loadGameState.AwakeState();
        _pauseState.AwakeState();
        _aerialState.AwakeState();
        _thirdPersonState.AwakeState();
        _atPOIState.AwakeState();
        _atTourStopState.AwakeState();
        _atCollectibleState.AwakeState();

        _fsm.SwitchedStateEvent += OnSwitchedState;
        _fsm.SetInitialState(_mainMenuState);

        return _fsm;
    }
    #endregion

    #region LIFE CYCLE
    protected override void Awake()
    {
        ServiceLocator.Instance.Register(this);
        _inputActions = new();

        base.Awake();
    }

    protected override void Start()
    {
        _scenesController = ServiceLocator.Instance.Get<ScenesController>();
        _scenesController.SceneLoadedPartiallyEvent += OnSceneLoadedPartially;
        _scenesController.ScenesLoadedFullyEvent += OnScenesLoadedFully;

        _inputActions.UI.Pause.performed += OnPauseInput;
        _inputActions.UI.CloseContextualPanel.performed += OnCloseContextualPanelInput;
        _inputActions.Camera.ExitMode.performed += OnChangeCameraInput;

        _uiSystem = ServiceLocator.Instance.Get<UISystem>();

        _uiSystem.MainMenuState.NewGameClickedEvent += OnNewGameClicked;
        _uiSystem.MainMenuState.LoadGameClickedEvent += OnLoadGameClicked;
        _uiSystem.MainMenuState.SettingsClickedEvent += OnSettingsClicked;
        _uiSystem.MainMenuState.CreditsClickedEvent += OnCreditsClicked;
        _uiSystem.MainMenuState.QuitClickedEvent += OnQuitClicked;

        _uiSystem.SettingsMenuState.CloseClickedEvent += OnSettingsClosed;
        _uiSystem.SettingsMenuState.EdgeScrollingToggledEvent += OnEdgeScrollingToggled;
        _uiSystem.SettingsMenuState.MusicVolumeChangedEvent += OnMusicVolumeChanged;
        _uiSystem.SettingsMenuState.SFXVolumeChangedEvent += OnSFXVolumeChanged;
        _uiSystem.SettingsMenuState.ShowPOIsToggledEvent += OnPOIsVisualizationToggled;
        _uiSystem.SettingsMenuState.ShowControlsToggledEvent += OnControlsVisibilityToggled;

        _uiSystem.CreditsScreenState.CreditsClosedEvent += OnCreditsClosed;

        _uiSystem.PauseState.ResumeGameClickedEvent += OnResumeGameClicked;
        _uiSystem.PauseState.MainMenuClickedEvent += OnMainMenuClicked;
        _uiSystem.PauseState.SettingsClickedEvent += OnSettingsClicked;
        _uiSystem.PauseState.CreditsClickedEvent += OnCreditsClicked;
        _uiSystem.PauseState.QuitClickedEvent += OnQuitClicked;

        _uiSystem.AerialHUDState.PreviousMilestoneClickedEvent += OnPreviousMilestoneClicked;
        _uiSystem.AerialHUDState.NextMilestoneClickedEvent += OnNextMilestoneClicked;
        _uiSystem.AerialHUDState.ModernVisualizationToggled += OnModernVisualizationToggled;
        _uiSystem.AerialHUDState.MilestoneInfoClickedEvent += OnMilestoneInfoClicked;
        _uiSystem.AerialHUDState.PauseClickedEvent += OnPauseClicked;

        _uiSystem.InformationDisplayState.ClosedEvent += OnContextualPanelClosed;
        _uiSystem.InformationDisplayState.PlayTourClickedEvent += OnPlayTourClicked;
        _uiSystem.InformationDisplayState.ResetTourClickedEvent += OnResetTourClicked;
        _uiSystem.InformationDisplayState.PauseClickedEvent += OnPauseClicked;

        _soundSystem = ServiceLocator.Instance.Get<SoundSystem>();

        _progressManager = ServiceLocator.Instance.Get<ProgressManager>();
        _progressManager.MilestoneChangedEvent += OnMilestoneChanged;

        base.Start();
    }

    void OnChangeCameraInput(InputAction.CallbackContext context)
    {
        SwitchToAerialState();
    }

    protected override void Update()
    {
        if (_gameSimulationSpeed != Time.timeScale && Time.timeScale != 0f)
            SetSimulationSpeed(_gameSimulationSpeed);
    }

    void OnDisable()
    {
        ServiceLocator.Instance.Unregister(this);
    }

    void OnDestroy()
    {
        _inputActions = null;
    }
    #endregion

    #region STATES HANDLERS
    public void SwitchToMainMenuState() => _fsm.SwitchState(_mainMenuState);
    public void SwitchToLoadGameState() => _fsm.SwitchState(_loadGameState);
    public void SwitchToPauseState() => _fsm.SwitchState(_pauseState);
    public void SwitchToAerialState() => _fsm.SwitchState(_aerialState);
    public void SwitchToThirdPersonState() => _fsm.SwitchState(_thirdPersonState);
    public void SwitchToAtPOIState(DataSO data, OrbitalCameraSettings orbitalCameraSettings)
    {
        _atPOIState.Data = data;
        _atPOIState.OrbitalCameraSettings = orbitalCameraSettings;
        _fsm.SwitchState(_atPOIState);
    }
    public void SwitchToAtTourStopState(TourStop tourStop)
    {
        _atTourStopState.TourStop = tourStop;
        _fsm.SwitchState(_atTourStopState);
    }
    public void SwitchToAtCollectibleState(Collectible collectible)
    {
        _atCollectibleState.Collectible = collectible;
        _fsm.SwitchState(_atCollectibleState);
    }
    #endregion

    #region PUBLIC METHODS
    public void SetSimulationSpeed(float speed)
    {
        // Validate input
        if (float.IsNaN(speed) || float.IsInfinity(speed))
        {
            Debug.LogWarning("GameManager.SetSimulationSpeed: invalid speed (NaN or Infinity). Change ignored.");
            return;
        }

        // Keep inside sensible bounds (aligns with inspector limits but slightly more permissive for safety)
        const float minSpeed = 0.01f;
        const float maxSpeed = 10f;
        float clamped = Mathf.Clamp(speed, minSpeed, maxSpeed);
        if (!Mathf.Approximately(clamped, speed))
            Debug.LogWarning($"GameManager: requested simulation speed {speed} was clamped to {clamped}.");

        _gameSimulationSpeed = clamped;

        // Apply time scale for gameplay
        Time.timeScale = _gameSimulationSpeed;

        // Keep physics timestep consistent with timeScale (default fixedDeltaTime is 0.02)
        Time.fixedDeltaTime = 0.02f * Mathf.Max(Time.timeScale, minSpeed);
    }
    #endregion

    #region PRIVATE METHODS
    public void LoadMainMenuScene()
    {
        // Load Main Menu Scene, if not already loaded
        if (!SceneManager.GetSceneByName(SceneDatabase.SceneName.MainMenuScene.ToString()).isLoaded)
            _scenesController.NewTransitionPlan()
                .Load(SceneDatabase.SceneType.Session, SceneDatabase.SceneName.MainMenuScene, setActive: true)
                .Unload(SceneDatabase.SceneType.Milestone) // Unload Milestone scene if it was loaded in a previous session
                .WithOverlay()
                .ClearAssets()
                .Perform();
    }

    public void LoadGame()
    {
        // Load Game Scene, if not already loaded
        if (!SceneManager.GetSceneByName(SceneDatabase.SceneName.GameplayScene.ToString()).isLoaded)
            _scenesController.NewTransitionPlan()
                .Load(SceneDatabase.SceneType.Session, SceneDatabase.SceneName.GameplayScene)
                .Load(SceneDatabase.SceneType.Milestone, ProgressManager.StoredMilestoneScene, setActive: true)
                .WithOverlay()
                .ClearAssets()
                .Perform();
    }

    public void LoadMilestone(SceneDatabase.SceneName milestoneToLoad)
    {
        _scenesController.NewTransitionPlan()
            .Load(SceneDatabase.SceneType.Milestone, milestoneToLoad, setActive: true)
            .WithOverlay()
            .ClearAssets()
            .Perform();
    }
    #endregion

    #region CALLBACK METHODS
    void OnSwitchedState()
    {
        StateChangedEvent?.Invoke();
    }

    void OnSceneLoadedPartially(SceneDatabase.SceneType type, SceneDatabase.SceneName name)
    {
        if (name == SceneDatabase.SceneName.GameplayScene)
        {
            _cameraSystem = ServiceLocator.Instance.Get<CameraSystem>();

            _tourManager = ServiceLocator.Instance.Get<TourManager>();
            _tourManager.TourStopVisitedEvent += OnTourStopVisited;

            _collectiblesManager = ServiceLocator.Instance.Get<CollectiblesManager>();
            _collectiblesManager.OnCollectibleFoundEvent += OnCollectibleFound;
        }
    }

    void OnScenesLoadedFully(Dictionary<SceneDatabase.SceneType, SceneDatabase.SceneName> loadedScenes, List<SceneDatabase.SceneType> unloadedTypes)
    {
        // A milestone scene loaded
        if (loadedScenes.TryGetValue(SceneDatabase.SceneType.Milestone, out var milestoneScene))
        {
            _playableCharacter = ServiceLocator.Instance.Get<PlayableCharacter>();
            // TODO necessary?
            _playableCharacter.PositionResetEvent += SwitchToThirdPersonState;
            SwitchToAerialState();
        }
    }

    void OnNewGameClicked()
    {
        GameSaveSystem.ClearAllData();
        SwitchToLoadGameState();
    }

    void OnLoadGameClicked()
    {
        SwitchToLoadGameState();
    }

    void OnSettingsClicked()
    {
        _uiSystem.SwitchToSettingsMenuState();
    }
    void OnCreditsClicked()
    {
        _uiSystem.SwitchToCreditsScreenState();
    }

    void OnQuitClicked()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // For convenience in the editor
#endif
    }

    void OnSettingsClosed()
    {
        if (IsInMainMenuState)
            _uiSystem.SwitchToMainMenuState();
        else if (IsInPauseState)
            _uiSystem.SwitchToPauseState();
    }

    void OnEdgeScrollingToggled(bool value)
    {
        _edgeScrollingValueSet = value;
        EdgeScrollingToggledEvent?.Invoke(value);
    }

    void OnMusicVolumeChanged(float value)
    {
        _musicVolumeValueSet = value;
        MusicVolumeChangedEvent?.Invoke(value);
    }

    void OnSFXVolumeChanged(float value)
    {
        _sfxVolumeValueSet = value;
        SFXVolumeChangedEvent?.Invoke(value);
    }

    void OnPOIsVisualizationToggled(bool value)
    {
        _POIsVisibilityValueSet = value;
        POIsVisualizationToggledEvent?.Invoke(value);
    }

    void OnControlsVisibilityToggled(bool value)
    {
        _controlsVisibilityValueSet = value;
        ControlsVisibilityToggledEvent?.Invoke(value);
    }

    void OnCreditsClosed()
    {
        if (IsInMainMenuState)
            _uiSystem.SwitchToMainMenuState();
        else if (IsInPauseState)
            _uiSystem.SwitchToPauseState();
    }

    void OnPauseInput(InputAction.CallbackContext context)
    {
        if (IsInPauseState)
            OnResumeGameClicked();
        else
            SwitchToPauseState();
    }

    void OnPauseClicked()
    {
        SwitchToPauseState();
    }

    void OnResumeGameClicked()
    {
        // TODO return to collectibleState
        if (_cameraSystem.IsInAerialState)
            SwitchToAerialState();
        else if (_cameraSystem.IsInThirdPersonState)
            SwitchToThirdPersonState();
        else if (_cameraSystem.IsInOrbitalState)
            SwitchToAtPOIState(_atPOIState.Data, _atPOIState.OrbitalCameraSettings);
        else if (_cameraSystem.IsInTourStopState)
            SwitchToAtTourStopState(_atTourStopState.TourStop);
    }

    void OnMainMenuClicked()
    {
        SwitchToMainMenuState();
    }

    void OnCloseContextualPanelInput(InputAction.CallbackContext context)
    {
        OnContextualPanelClosed();
    }

    void OnPreviousMilestoneClicked()
    {
        _loadGameState.MilestoneToLoad = _progressManager.SwitchToPreviousMilestone().SceneName;
        SwitchToLoadGameState();
    }

    void OnNextMilestoneClicked()
    {
        _loadGameState.MilestoneToLoad = _progressManager.SwitchToNextMilestone().SceneName;
        SwitchToLoadGameState();
    }

    void OnModernVisualizationToggled(bool value)
    {
        _modernVisualizationValueSet = value;
        ModernVisualizationToggled?.Invoke(value);
    }

    void OnMilestoneInfoClicked()
    {
        _uiSystem.SwitchToInformationDisplayState(_progressManager.CurrentMilestoneData);
    }

    void OnContextualPanelClosed()
    {
        if (IsAtPOIState)
            SwitchToAerialState();
        else if (IsAtTourStopState || IsAtCollectibleState)
            SwitchToThirdPersonState();
    }

    void OnPlayTourClicked()
    {
        PlayTourClickedEvent?.Invoke();
        SwitchToThirdPersonState();
    }

    void OnResetTourClicked()
    {
        ResetTourClickedEvent?.Invoke();
        SwitchToThirdPersonState();
    }

    void OnMilestoneChanged(Milestone_DataSO milestoneData) => MilestoneChangedEvent?.Invoke(milestoneData);

    void OnTourStopVisited(TourStop tourStop)
    {
        if (tourStop.Data == null) return;
        if (!tourStop.gameObject.activeInHierarchy)
        {
            Debug.LogWarning($"TourStop '{tourStop.name}' is not active in hierarchy.");
            return;
        }

        SwitchToAtTourStopState(tourStop);
    }

    void OnCollectibleFound(Collectible collectible)
    {
        if (collectible.Data == null) return;
        if (!collectible.gameObject.activeInHierarchy)
        {
            Debug.LogWarning($"Collectible '{collectible.name}' is not active in hierarchy.");
            return;
        }

        SwitchToAtCollectibleState(collectible);
    }
    #endregion
}
