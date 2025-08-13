using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class UIManager : Singleton<UIManager>
{
    #region PUBLIC PROPERTIES
    public UIDocument _UIDocument;
    [Header("User Interface Properties")]
    public Vector2 _tooltipOffset = new(-30, -30);

    // State Machine
    public StackFiniteStateMachine<UIManager> _fsm;
    public MainMenu_UIState _mainMenuState;
    public SpectatorHUD_UIState _spectatorHUDState;
    public PlayerHUD_UIState _playerHUDState;
    public PauseMenu_UIState _pauseState;
    public HeritageMenu_UIState _heritageState;
    #endregion

    #region PRIVATE PROPERTIES
    #endregion

    #region INHERITED
    protected override void OnAwake()
    {
        // Singleton
        base.OnAwake();

        _UIDocument = GetComponent<UIDocument>();
    }

    protected override void OnStart()
    {

    }

    protected override void OnUpdate()
    {

    }

    protected override ADecisionSystem<UIManager> CreateDecisionSystem()
    {
        _fsm = new(this);

        _mainMenuState = new(_fsm);
        _spectatorHUDState = new(_fsm);
        _playerHUDState = new(_fsm);
        _pauseState = new(_fsm);
        _heritageState = new(_fsm);

        // Set initial state based on scene name
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "GameScene")
            _fsm.SetInitialState(_spectatorHUDState);
        else
            _fsm.SetInitialState(_mainMenuState);

        return _fsm;
    }
    #endregion

    #region PUBLIC METHODS

    #endregion
}
