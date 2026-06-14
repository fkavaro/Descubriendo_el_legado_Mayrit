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

    // Game states
    public bool IsInMainMenuState => _fsm.IsCurrentState(_mainMenuState);
    public bool IsLoadGameState => _fsm.IsCurrentState(_loadGameState);
    public bool IsInPauseState => _fsm.IsCurrentState(_pauseState);
    public bool IsInGameplayState => _fsm.IsCurrentState(_gameplayState);
    public Gameplay_GameState GameplayState => _gameplayState;
    public bool IsInAerialState => _fsm.IsCurrentState(_gameplayState) && _gameplayState.IsInAerialState();
    public bool IsInThirdPersonState => _fsm.IsCurrentState(_gameplayState) && _gameplayState.IsInThirdPersonState();
    public bool IsAtPOIState => _fsm.IsCurrentState(_gameplayState) && _gameplayState.IsInAtPOIState();
    public bool IsAtTourStopState => _fsm.IsCurrentState(_gameplayState) && _gameplayState.IsInAtTourStopState();
    public bool IsAtCollectibleState => _fsm.IsCurrentState(_gameplayState) && _gameplayState.IsInAtCollectibleState();

    public float SimulationSpeed => _gameSimulationSpeed;

    // Progress system
    public List<Milestone_DataSO> MilestonesData => _progressSystem.MilestonesData;
    public Milestone_DataSO CurrentMilestoneData => MilestonesData[_currentMilestoneIndex];
    public int LastCompletedMilestoneIndex => _lastCompletedMilestoneIndex;
    public int CurrentMilestoneIndex => _currentMilestoneIndex;
    public int CompletedMilestonesCount => _lastCompletedMilestoneIndex + 1;
    public int TotalMilestonesCount => MilestonesData.Count;
    public bool IsCurrentMilestoneCompleted => _canSkipMilestones || _tourManager.CurrentTour.IsCompleted;
    public bool WasCurrentMilestoneCompleted => _currentMilestoneIndex <= _lastCompletedMilestoneIndex;
    public SceneDatabase.SceneName StoredMilestoneScene => MilestonesData[GetStoredMilestone()].SceneName;

    // Settings
    public bool IsEdgeScrollingMovementEnabled => _IsEdgeScrollingMovementEnabled;
    public bool ArePOIsVisualized => _arePOIsVisualized;
    public bool AreControlsMappingsDisplayed => _areControlsMappingDisplayed;
    public bool AreModernBuildingsVisualized => _areModernBuildingsDisplayed;
    public bool CanSkipMilestones => _canSkipMilestones;
    public float MusicVolume => _musicVolume;
    public float SFXVolume => _sfxVolume;

    // Systems dependencies
    public ScenesController ScenesController => _scenesController;
    public UISystem UISystem => _uiSystem;
    public SoundSystem SoundSystem => _soundSystem;
    public CameraSystem CameraSystem => _cameraSystem;
    public ProgressSystem ProgressSystem => _progressSystem;
    public PlayableCharacter PlayableCharacter => _playableCharacter;
    public TourManager TourManager => _tourManager;
    public CollectiblesManager CollectiblesManager => _collectiblesManager;
    #endregion

    #region EDITOR PROPERTIES
    [SerializeField, ReadOnly] protected string _currentGameplayState = "";
    [Tooltip("Game simulation speed multiplier. Set by Camera states.")]
    [Range(0.1f, 10f)]
    [SerializeField] float _gameSimulationSpeed = 1f;

    [Header("Progress")]
    [Range(-1, 7)]
    [SerializeField] int _lastCompletedMilestoneIndex = -1;
    [Range(0, 7)]
    [SerializeField] int _currentMilestoneIndex = 0;

    [Header("Settings values")]
    [SerializeField] bool _IsEdgeScrollingMovementEnabled = true;
    [SerializeField] bool _arePOIsVisualized = true;
    [SerializeField] bool _areControlsMappingDisplayed = true;
    [SerializeField] bool _areModernBuildingsDisplayed = false;
    [SerializeField] bool _canSkipMilestones = false;
    [SerializeField] float _musicVolume = 1f;
    [SerializeField] float _sfxVolume = 1f;
    #endregion

    #region EVENTS
    public event Action StateChangedEvent;

    // UI Events
    public event Action<bool> EdgeScrollingToggledEvent;
    public event Action<float> MusicVolumeChangedEvent;
    public event Action<float> SFXVolumeChangedEvent;
    public event Action<bool> ModernVisualizationToggled;
    public event Action<bool> POIsDisplayToggledEvent;
    public event Action<bool> ControlsMappingsDisplayToggledEvent;
    public event Action<bool> MilestoneSkipToggledEvent;
    public event Action PlayTourClickedEvent;
    public event Action TourAndPlayerResetEvent;

    // Progress Events
    public event Action<Milestone_DataSO> MilestoneChangedEvent;
    #endregion

    #region INTERNAL PROPERTIES
    GameInputActions _inputActions;
    FiniteStateMachine<AGameState> _fsm;
    MainMenu_GameState _mainMenuState;
    LoadGame_GameState _loadGameState;
    Pause_GameState _pauseState;
    Gameplay_GameState _gameplayState;

    ScenesController _scenesController;
    UISystem _uiSystem;
    SoundSystem _soundSystem;
    CameraSystem _cameraSystem;
    PlayableCharacter _playableCharacter;
    ProgressSystem _progressSystem;
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
        _gameplayState = new(this);

        _gameplayState.AwakeState();
        _gameplayState.Fsm.SwitchedStateEvent += OnSwitchedState;

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

        InputActions.UI.Enable();
        _inputActions.UI.Pause.performed += OnPauseInput;
        _inputActions.UI.GoBack.performed += OnGoBackInput;

        _uiSystem = ServiceLocator.Instance.Get<UISystem>();

        _uiSystem.POISelectedEvent += OnPOISelected;
        _uiSystem.TourResetEvent += OnTourResetted;

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
        _uiSystem.SettingsMenuState.MilestoneSkipToggledEvent += OnMilestoneSkipToggled;

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

        _progressSystem = ServiceLocator.Instance.Get<ProgressSystem>();
        _progressSystem.MilestoneChangedEvent += OnMilestoneChanged;

        _tourManager = ServiceLocator.Instance.Get<TourManager>();
        _tourManager.TourStopVisitedEvent += OnTourStopVisited;
        _tourManager.TourCompletedEvent += OnTourCompleted;

        _collectiblesManager = ServiceLocator.Instance.Get<CollectiblesManager>();
        _collectiblesManager.OnCollectibleFoundEvent += OnCollectibleFound;

        base.Start();
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
    public void SwitchToMainMenuState()
    {
        _gameplayState.Fsm.Reset();
        _fsm.SwitchState(_mainMenuState);
    }
    public void SwitchToLoadGameState() => _fsm.SwitchState(_loadGameState);
    public void SwitchToPauseState() => _fsm.SwitchState(_pauseState);
    public void SwitchToGameplayState() => _fsm.SwitchState(_gameplayState);
    public void SwitchToAerialState()
    {
        if (!IsInGameplayState)
            SwitchToGameplayState();

        _gameplayState.SwitchToAerialState();
    }
    public void SwitchToThirdPersonState()
    {
        if (!IsInGameplayState)
            SwitchToGameplayState();

        _gameplayState.SwitchToThirdPersonState();
    }
    public void SwitchToAtPOIState(DataSO data, OrbitalCameraSettings orbitalCameraSettings)
    {
        if (!IsInGameplayState)
            SwitchToGameplayState();

        _gameplayState.SwitchToAtPOIState(data, orbitalCameraSettings);
    }
    public void SwitchToAtTourStopState(TourStop tourStop)
    {
        if (!IsInGameplayState)
            SwitchToGameplayState();

        _gameplayState.SwitchToAtTourStopState(tourStop);
    }
    public void SwitchToAtCollectibleState(Collectible collectible)
    {
        if (!IsInGameplayState)
            SwitchToGameplayState();

        _gameplayState.SwitchToAtCollectibleState(collectible);
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
                .Load(SceneDatabase.SceneType.Milestone, StoredMilestoneScene, setActive: true)
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

    public bool AtFirstMilestone() => _progressSystem.AtFirstMilestone();
    public bool AtLastMilestone() => _progressSystem.AtLastMilestone();
    #endregion

    #region PRIVATE METHODS
    int GetStoredMilestone()
    {
        PlayerProgressData saveData = GameSaveSystem.LoadAllData();
        _lastCompletedMilestoneIndex = Mathf.Clamp(saveData.StoredMilestoneIndex, -1, MilestonesData.Count - 1); // Could be -1 if no valid data found
        _currentMilestoneIndex = _lastCompletedMilestoneIndex;

        if (_lastCompletedMilestoneIndex < MilestonesData.Count - 1) // Not at last milestone
            _currentMilestoneIndex++; // To load the next milestone to be completed

        if (DebugMode)
            Debug.Log($"[ProgressSystem] Milestone Change index loaded {_currentMilestoneIndex} ({CurrentMilestoneData.Header}).");

        return _currentMilestoneIndex;
    }

    void UpdateCompletedMilestoneIndex()
    {
        _lastCompletedMilestoneIndex = Mathf.Max(_lastCompletedMilestoneIndex, _currentMilestoneIndex);
    }

    void SaveProgress()
    {
        GameSaveSystem.SaveMilestoneIdx(_lastCompletedMilestoneIndex);
        if (DebugMode)
            Debug.Log($"ProgressSystem: Progress saved. Highest completed milestone index: {_lastCompletedMilestoneIndex}");
    }
    #endregion

    #region STATE CALLBACK METHOD
    void OnSwitchedState()
    {
        CurrentAction = _fsm.CurrentState?.StateName ?? "None";
        _currentGameplayState = _gameplayState.Fsm.CurrentState?.StateName ?? "None";
        StateChangedEvent?.Invoke();
    }
    #endregion

    #region SCENE LOADING CALLBACK METHODS
    void OnSceneLoadedPartially(SceneDatabase.SceneType type, SceneDatabase.SceneName name)
    {
        if (name == SceneDatabase.SceneName.GameplayScene)
        {
            _cameraSystem = ServiceLocator.Instance.Get<CameraSystem>();
        }
    }

    void OnScenesLoadedFully(Dictionary<SceneDatabase.SceneType, SceneDatabase.SceneName> loadedScenes, List<SceneDatabase.SceneType> unloadedTypes)
    {
        // A milestone scene loaded
        if (loadedScenes.TryGetValue(SceneDatabase.SceneType.Milestone, out var milestoneScene))
        {
            _playableCharacter = ServiceLocator.Instance.Get<PlayableCharacter>();

            SwitchToAerialState();
        }
    }
    #endregion

    #region PROGRESS CALLBACK METHODS
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

    void OnTourCompleted(Tour tour)
    {
        UpdateCompletedMilestoneIndex();
        SaveProgress();
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

    #region UI CALLBACK METHODS
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
        _IsEdgeScrollingMovementEnabled = value;
        EdgeScrollingToggledEvent?.Invoke(value);
    }

    void OnMusicVolumeChanged(float value)
    {
        _musicVolume = value;
        MusicVolumeChangedEvent?.Invoke(value);
    }

    void OnSFXVolumeChanged(float value)
    {
        _sfxVolume = value;
        SFXVolumeChangedEvent?.Invoke(value);
    }

    void OnPOIsVisualizationToggled(bool value)
    {
        _arePOIsVisualized = value;
        POIsDisplayToggledEvent?.Invoke(value);
    }

    void OnControlsVisibilityToggled(bool value)
    {
        _areControlsMappingDisplayed = value;
        ControlsMappingsDisplayToggledEvent?.Invoke(value);
    }

    void OnMilestoneSkipToggled(bool value)
    {
        _canSkipMilestones = value;
        MilestoneSkipToggledEvent?.Invoke(value);
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
        SwitchToGameplayState();
    }

    void OnMainMenuClicked()
    {
        SwitchToMainMenuState();
    }

    void OnGoBackInput(InputAction.CallbackContext context)
    {
        if (IsInMainMenuState)
        {
            if (_uiSystem.IsInSettingsMenuState)
                OnSettingsClosed();
            else if (_uiSystem.IsInCreditsScreenState)
                OnCreditsClosed();
        }
        else if (IsInPauseState)
        {
            if (_uiSystem.IsInSettingsMenuState)
                OnSettingsClosed();
            else if (_uiSystem.IsInCreditsScreenState)
                OnCreditsClosed();
        }
        else if (IsInGameplayState)
        {
            if (IsInThirdPersonState)
                SwitchToAerialState();
            else
                OnContextualPanelClosed();
        }
    }

    void OnPreviousMilestoneClicked()
    {
        _currentMilestoneIndex = _progressSystem.SwitchToPreviousMilestone();
        _loadGameState.MilestoneToLoad = CurrentMilestoneData.SceneName;
        SwitchToLoadGameState();
    }

    void OnNextMilestoneClicked()
    {
        _currentMilestoneIndex = _progressSystem.SwitchToNextMilestone();
        _loadGameState.MilestoneToLoad = CurrentMilestoneData.SceneName;
        SwitchToLoadGameState();
    }

    void OnModernVisualizationToggled(bool value)
    {
        _areModernBuildingsDisplayed = value;
        ModernVisualizationToggled?.Invoke(value);
    }

    void OnMilestoneInfoClicked()
    {
        // TODO orbital state around whole city
        _uiSystem.SwitchToInformationDisplayState(CurrentMilestoneData);
    }

    void OnContextualPanelClosed()
    {
        if (IsAtPOIState)
            SwitchToAerialState();
        else if (IsAtTourStopState || IsAtCollectibleState)
            SwitchToThirdPersonState();
        // TODO should'nt be necessary if orbiting aorund city when clicked milestone info
        else if (IsInAerialState && !_uiSystem.IsInAerialHUDState)
            _uiSystem.SwitchToAerialHUDState();
        else if (IsInPauseState && (_uiSystem.IsInSettingsMenuState || _uiSystem.IsInCreditsScreenState))
            _uiSystem.SwitchToPauseState();
    }

    void OnPlayTourClicked()
    {
        PlayTourClickedEvent?.Invoke();
        SwitchToThirdPersonState();
    }

    void OnResetTourClicked()
    {
        StartCoroutine(_uiSystem.ResetTourAndPlayerFadeInCoroutine());
    }

    void OnTourResetted()
    {
        TourAndPlayerResetEvent?.Invoke();
        SwitchToThirdPersonState();
    }

    void OnPOISelected(DataSO data, OrbitalCameraSettings orbitalSettings)
    {
        _soundSystem.PlayButtonClickSFX();
        SwitchToAtPOIState(data, orbitalSettings);
    }
    #endregion
}
