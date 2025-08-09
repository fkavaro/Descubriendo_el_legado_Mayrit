using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

public class PlayerHUD_UIState : AUIState
{
    #region PUBLIC PROPERTIES
    #endregion

    #region PRIVATE PROPERTIES
    //Label _tooltip, _contextualPanelName, _contextualPanelDescription;
    Button _pauseButton, _playerButton;
    VisualElement _activityArea;
    #endregion

    #region INHERITED
    public PlayerHUD_UIState(StackFiniteStateMachine<UIManager> stateMachine)
    : base("PlayerHUD", stateMachine) { }

    public override void AwakeState()
    {
        _UIDocument = UIManager.Instance._UIDocument;
        _pauseButton = _UIDocument.rootVisualElement.Q<Button>("PauseButton");
        _playerButton = _UIDocument.rootVisualElement.Q<Button>("PlayerButton");
        _activityArea = _UIDocument.rootVisualElement.Q<VisualElement>("ActivityArea");

        _screen = _UIDocument.rootVisualElement.Q<VisualElement>("PlayerHUD");

        if (_pauseButton == null)
            Debug.LogWarning("_pauseButton not found");
        if (_activityArea == null)
            Debug.LogWarning("_activityArea not found");
        if (_playerButton == null)
            Debug.LogWarning("_playerButton button not found");

        _pauseButton.RegisterCallback<ClickEvent>(SwitchToPauseState);
        _playerButton.RegisterCallback<ClickEvent>(SwitchToSpectatorHUDState);
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

    void SwitchToSpectatorHUDState(ClickEvent evt)
    {
        if (_playerButton == null) return;

        _stateMachine.SwitchState(UIManager.Instance._spectatorHUDState);
        CameraManager.Instance.ToggleCameraState();
    }
    #endregion
}