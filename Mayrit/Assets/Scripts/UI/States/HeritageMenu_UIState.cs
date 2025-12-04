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
        _gameManager.SwitchToPauseState();
    }
    #endregion

    #region CALLBACK METHODS
    void OnPlayClicked(ClickEvent evt)
    {
        _uiManager.BehaviourSystem.SwitchToPreviousStateInStack(); // Switch to previous state: player or spectator HUD
        _gameManager.SwitchToGamePlayState();
    }
    #endregion
}
