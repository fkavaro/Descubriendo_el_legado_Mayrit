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
    public HeritageMenu_UIState(UIDocument uiDocument)
    : base("HeritageMenu", uiDocument) { }

    #region INHERITED
    public override void StartState()
    {
        _screen = _UIDocument.rootVisualElement.Q<VisualElement>("HeritageMenu");
        _playButton = _screen.Q<Button>("PlayButton");

        _playButton.RegisterCallback<ClickEvent>(SwitchToHUDState);

        _screen.style.display = DisplayStyle.Flex; // Show 

        // Game pause state
        GameManager.Instance.BehaviourSystem.SwitchState(GameManager.Instance._pauseState);
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
        UIManager.Instance.BehaviourSystem.SwitchToPreviousStateInStack(); // Switch to previous state: player or spectator HUD
        GameManager.Instance.BehaviourSystem.SwitchState(GameManager.Instance._gamePlayState);
    }
    #endregion
}
