using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class UISystem : ABehaviourEntity<StackFiniteStateMachine<AUIState>>
{
    #region PROPERTY HELPERS

    public bool IsInMainMenuState => _sfsm.IsCurrentState(_mainMenuState);
    public MainMenu_UIState MainMenuState => _mainMenuState;
    public bool IsInAerialHUDState => _sfsm.IsCurrentState(_aerialHUDState);
    public AerialHUD_UIState AerialHUDState => _aerialHUDState;
    public bool IsInPlayerHUDState => _sfsm.IsCurrentState(_playerHUDState);
    public PlayerHUD_UIState PlayerHUDState => _playerHUDState;
    public bool IsInPauseState => _sfsm.IsCurrentState(_pauseState);
    public PauseMenu_UIState PauseState => _pauseState;
    public bool IsInInformationDisplayState => _sfsm.IsCurrentState(_informationDisplayState);
    public InformationDisplay_UIState InformationDisplayState => _informationDisplayState;
    public bool IsInSettingsMenuState => _sfsm.IsCurrentState(_settingsMenuState);
    public SettingsMenu_UIState SettingsMenuState => _settingsMenuState;
    public bool IsInLoadingScreenState => _sfsm.IsCurrentState(_loadingScreenState);
    public LoadingScreen_UIState LoadingScreenState => _loadingScreenState;
    public bool IsInCreditsScreenState => _sfsm.IsCurrentState(_creditsScreenState);
    public CreditsScreen_UIState CreditsScreenState => _creditsScreenState;

    public UIDocument UIDocument => _uiDocument;
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
    #endregion

    #region INTERNAL PROPERTIES
    UIDocument _uiDocument;
    ContextualPanelComponent _contextualPanelComponent;

    // Events
    public event Action StateChangedEvent;

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
    GameManager _gameManager;
    ProgressManager _progressManager;
    #endregion

    #region INHERITED
    public override StackFiniteStateMachine<AUIState> DefineBehaviourSystem()
    {
        _sfsm = new(this);

        _uiDocument = GetComponent<UIDocument>();

        _contextualPanelComponent = new(this, _uiDocument, _fadeInDuration, _fadeOutDuration);
        _contextualPanelComponent.AwakeState();

        // States initialization
        _mainMenuState = new(this, _uiDocument, _fadeInDuration, _fadeOutDuration);
        _aerialHUDState = new(this, _uiDocument, _fadeInDuration * 5f, _fadeOutDuration);
        _playerHUDState = new(this, _uiDocument, _fadeInDuration, _fadeOutDuration);
        _pauseState = new(this, _uiDocument, _fadeInDuration, _fadeOutDuration);
        _informationDisplayState = new(this, _uiDocument, _fadeInDuration, _fadeOutDuration, _contextualPanelComponent);
        _settingsMenuState = new(this, _uiDocument, 0f, 0f);
        _loadingScreenState = new(this, _uiDocument, _fadeInDuration, _fadeOutDuration, _contextualPanelComponent);
        _creditsScreenState = new(this, _uiDocument, 0f, 0f);

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
        _gameManager = ServiceLocator.Instance.Get<GameManager>();
        _progressManager = ServiceLocator.Instance.Get<ProgressManager>();

        //base.Start(); When main menu scene is loaded
    }

    void OnDisable()
    {
        ServiceLocator.Instance.Unregister(this);
    }
    #endregion

    #region STATE HANDLERS
    public void SwitchToMainMenuState() => _sfsm?.SwitchState(_mainMenuState);
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
    #endregion

    #region CALLBACK METHODS
    void OnSwitchedState()
    {
        StateChangedEvent?.Invoke();
    }

    void OnResetTourClicked()
    {
        StartCoroutine(ResetTourWithBlackFadeCoroutine());
    }

    IEnumerator ResetTourWithBlackFadeCoroutine()
    {
        yield return FadeInBlackLoadingScreenCoroutine();
        _playerHUDState.ShownCompletedTourVisual = false;
        // TODO check this event
        //ResetTourClickedEvent?.Invoke();
        yield return new WaitForSeconds(_fadeInDuration);
        yield return FadeOutBlackLoadingScreenCoroutine();
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
    #endregion
}
