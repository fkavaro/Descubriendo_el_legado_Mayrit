using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

public class PlayerHUD_UIState : AUIState
{
    #region PUBLIC PROPERTIES
    #endregion

    #region PRIVATE PROPERTIES
    Button _pauseButton;
    VisualElement _activityArea;
    #endregion

    #region INHERITED
    public PlayerHUD_UIState(StackFiniteStateMachine<UIManager> stateMachine)
    : base("PlayerHUD", stateMachine) { }

    public override void AwakeState()
    {
        _UIDocument = UIManager.Instance._UIDocument;
        _screen = _UIDocument.rootVisualElement.Q<VisualElement>("PlayerHUD");

        _pauseButton = _screen.Q<Button>("PauseButton");
        _activityArea = _screen.Q<VisualElement>("ActivityArea");

        if (_pauseButton == null)
            Debug.LogWarning("_pauseButton not found");
        if (_activityArea == null)
            Debug.LogWarning("_activityArea not found");

        _pauseButton.RegisterCallback<ClickEvent>(SwitchToPauseState);
    }

    public override void StartState()
    {
        _screen.style.display = DisplayStyle.Flex; // Show HUD
    }

    public override void UpdateState()
    {

    }

    public override void ExitState()
    {
        _screen.style.display = DisplayStyle.None; // Hide HUD
    }

    #endregion

    #region PUBLIC METHODS
    #endregion

    #region PRIVATE METHODS
    void SwitchToPauseState(ClickEvent evt)
    {
        _stateMachine.SwitchState(UIManager.Instance._pauseState);
    }
    #endregion
}