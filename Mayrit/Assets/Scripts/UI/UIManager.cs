using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class UIManager : Singleton<UIManager>
{
    #region PUBLIC PROPERTIES
    [Header("User Interface Properties")]
    public UIDocument UIDocument;
    public int tooltipOffsetX = 3, tooltipOffsetY = 40;

    // State Machine
    public FiniteStateMachine<UIManager> fsm;
    public MainMenu_UIState mainMenuState;
    public HUD_UIState hudState;
    public PauseMenu_UIState pauseState;
    #endregion

    #region PRIVATE PROPERTIES
    #endregion

    #region INHERITED
    protected override void OnAwake()
    {

    }

    protected override void OnStart()
    {

    }

    protected override void OnUpdate()
    {

    }

    protected override ADecisionSystem<UIManager> CreateDecisionSystem()
    {
        fsm = new(this);

        mainMenuState = new(fsm);
        hudState = new(fsm);
        pauseState = new(fsm);

        // Set initial state based on scene name
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "GameScene")
        {
            fsm.SetInitialState(hudState);
        }
        else
        {
            fsm.SetInitialState(mainMenuState);
        }

        return fsm;
    }
    #endregion

    #region PUBLIC METHODS

    #endregion
}
