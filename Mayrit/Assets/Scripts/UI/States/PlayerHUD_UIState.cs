using UnityEngine;
using UnityEngine.UIElements;

public class PlayerHUD_UIState : AHUDState
{
    #region  PROPERTIES
    Button _pauseButton;
    VisualElement _activityArea;
    Label _activityName,
        _activityDescription;
    #endregion

    #region CONSTRUCTOR
    public PlayerHUD_UIState(UIDocument uiDocument)
    : base("PlayerHUD", uiDocument) { }
    #endregion

    #region UI STATE INHERITED METHODS
    protected override void ConfigureUIElements()
    {
        _pauseButton = _screen.Q<Button>("PauseButton");
        _activityArea = _screen.Q<VisualElement>("ActivityArea");

        if (_pauseButton == null)
            Debug.LogWarning("_pauseButton not found");
        if (_activityArea == null)
            Debug.LogWarning("_activityArea not found");
    }

    protected override void RegisterCallbacks()
    {
        _pauseButton.RegisterCallback<ClickEvent>(OnPauseClicked);
    }
    #endregion

    #region HUD STATE INHERITED METHODS
    protected override void OnContextualPanelShown()
    {
        _activityArea.style.display = DisplayStyle.None;
    }

    protected override void OnContextualPanelHidden()
    {
        _activityArea.style.display = DisplayStyle.Flex;
    }
    #endregion

    #region CALLBACK METHODS
    void OnPauseClicked(ClickEvent evt)
    {
        UIManager.Instance.SwitchToPauseState();
    }
    #endregion
}