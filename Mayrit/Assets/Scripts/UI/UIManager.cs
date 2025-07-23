using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class UIManager : Singleton<UIManager>
{
    #region PUBLIC PROPERTIES
    public UIDocument _UIDocument;
    [Header("User Interface Properties")]
    public Vector2 _tooltipOffset = new(-30, -30);
    public Vector2 _playerButtonOffset = new(-85, -185);

    // State Machine
    public FiniteStateMachine<UIManager> _fsm;
    public MainMenu_UIState _mainMenuState;
    public HUD_UIState _hudState;
    public PauseMenu_UIState _pauseState;
    #endregion

    #region PRIVATE PROPERTIES
    #endregion

    #region INHERITED
    protected override void OnAwake()
    {
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
        _hudState = new(_fsm);
        _pauseState = new(_fsm);

        // Set initial state based on scene name
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "GameScene")
        {
            _fsm.SetInitialState(_hudState);
        }
        else
        {
            _fsm.SetInitialState(_mainMenuState);
        }

        return _fsm;
    }
    #endregion

    #region PUBLIC METHODS

    #endregion
}
