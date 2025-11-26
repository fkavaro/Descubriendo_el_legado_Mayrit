using UnityEngine;
using UnityEngine.UIElements;

public class HeritageMenu_UIState : AUIState
{
    #region PROPERTIES
    Button _playButton;
    #endregion

    #region CONSTRUCTOR
    public HeritageMenu_UIState(UIDocument uiDocument)
    : base("HeritageMenu", uiDocument) { }
    #endregion

    #region INHERITED METHODS
    protected override void ConfigureUIElements()
    {
        _playButton = _screen.Q<Button>("PlayButton");
    }

    protected override void RegisterCallbacks()
    {
        _playButton.RegisterCallback<ClickEvent>(OnPlayClicked);
    }

    protected override void OnStartState()
    {
        GameManager.Instance.SwitchToPauseState();
    }
    #endregion

    #region CALLBACK METHODS
    void OnPlayClicked(ClickEvent evt)
    {
        UIManager.Instance.BehaviourSystem.SwitchToPreviousStateInStack(); // Switch to previous state: player or spectator HUD
        GameManager.Instance.SwitchToGamePlayState();
    }
    #endregion
}
