using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{

    #region PUBLIC PROPERTIES
    // State Machine
    public FiniteStateMachine<GameManager> fsm;
    public MainMenu_GameState mainMenuState;
    public GamePlay_GameState gamePlayState;
    public Pause_GameState pauseState;

    public GameInputActions _inputActions;
    #endregion

    #region PRIVATE PROPERTIES

    #endregion

    #region INHERITED
    protected override void OnAwake()
    {
        _inputActions = new();
    }

    protected override void OnStart()
    {

    }

    protected override void OnUpdate()
    {

    }
    protected override ADecisionSystem<GameManager> CreateDecisionSystem()
    {
        fsm = new(this);

        mainMenuState = new(fsm);
        gamePlayState = new(fsm);
        pauseState = new(fsm);

        // Set initial state based on scene name
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "GameScene")
        {
            fsm.SetInitialState(gamePlayState);
        }
        else
        {
            fsm.SetInitialState(mainMenuState);
        }

        return fsm;
    }

    private void OnDestroy()
    {
        _inputActions?.Disable(); // Disables all action maps. To avoi errors
    }
    #endregion

    #region PUBLIC METHODS

    #endregion

    #region PRIVATE METHODS

    #endregion
}
