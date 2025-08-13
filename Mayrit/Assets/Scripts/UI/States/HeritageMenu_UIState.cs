using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

public class HeritageMenu_UIState : AUIState
{
    #region PUBLIC PROPERTIES
    #endregion

    #region PRIVATE PROPERTIES
    Button _playButton;
    #endregion

    // Constructor
    public HeritageMenu_UIState(StackFiniteStateMachine<UIManager> stateMachine)
    : base("HeritageMenu", stateMachine) { }

    #region INHERITED
    public override void AwakeState()
    {
        _UIDocument = UIManager.Instance._UIDocument;
        _screen = _UIDocument.rootVisualElement.Q<VisualElement>("HeritageMenu");
        _playButton = _screen.Q<Button>("PlayButton");

        _playButton.RegisterCallback<ClickEvent>(SwitchToHUDState);
    }

    public override void StartState()
    {
        _screen.style.display = DisplayStyle.Flex; // Show 

        // Game pause state
        GameManager.Instance._fsm.SwitchState(GameManager.Instance._pauseState);
    }
    public override void ExitState()
    {
        _screen.style.display = DisplayStyle.None; // Hide
    }
    #endregion

    #region PUBLIC METHODS

    #endregion

    #region PRIVATE METHODS
    void SwitchToHUDState(ClickEvent evt)
    {
        _stateMachine.SwitchToPreviousStateInStack(); // Switch to previous state: player or spectator HUD
        GameManager.Instance._fsm.SwitchState(GameManager.Instance._gamePlayState);
    }
    #endregion
}
