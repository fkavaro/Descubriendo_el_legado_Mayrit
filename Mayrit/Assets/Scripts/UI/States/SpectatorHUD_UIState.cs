using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

public class SpectatorHUD_UIState : AUIState
{
    #region PUBLIC PROPERTIES
    #endregion

    #region PRIVATE PROPERTIES
    Label _tooltip, _contextualPanelName, _contextualPanelDescription, _eventName, _eventDate;
    Button _pauseButton, _closeContextualPanelButton, _eventInfoButton, _playerButton;
    VisualElement _contextualPanel, _eventArea;
    Vector2 _cursorScreenPos;
    #endregion

    #region INHERITED
    public SpectatorHUD_UIState(FiniteStateMachine<UIManager> stateMachine)
    : base("SpectatorHUD", stateMachine) { }

    public override void AwakeState()
    {
        _UIDocument = UIManager.Instance._UIDocument;
        _pauseButton = _UIDocument.rootVisualElement.Q<Button>("PauseButton");
        _playerButton = _UIDocument.rootVisualElement.Q<Button>("PlayerButton");

        _screen = _UIDocument.rootVisualElement.Q<VisualElement>("SpectatorHUD");
        _tooltip = _screen.Q<Label>("Tooltip");
        _eventArea = _screen.Q<VisualElement>("EventArea");
        _eventInfoButton = _eventArea.Q<Button>("InfoButton");
        _eventName = _eventArea.Q<Label>("Name");
        _eventDate = _eventArea.Q<Label>("Date");
        _contextualPanel = _screen.Q<VisualElement>("ContextualPanel");
        _contextualPanelName = _contextualPanel.Q<Label>("Name");
        _contextualPanelDescription = _contextualPanel.Q<Label>("Description");
        _closeContextualPanelButton = _contextualPanel.Q<Button>("CloseButton");

        if (_tooltip == null)
            Debug.LogWarning("_tooltip not found");
        if (_pauseButton == null)
            Debug.LogWarning("_pauseButton not found");
        if (_contextualPanel == null)
            Debug.LogWarning("_contextualPanel not found");
        if (_contextualPanelName == null)
            Debug.LogWarning("_contextualPanelName not found");
        if (_contextualPanelDescription == null)
            Debug.LogWarning("_contextualPanelDescription not found");
        if (_closeContextualPanelButton == null)
            Debug.LogWarning("_closeContextualPanelButton button not found");
        if (_eventInfoButton == null)
            Debug.LogWarning("_eventInfoButton button not found");
        if (_eventArea == null)
            Debug.LogWarning("_eventArea button not found");
        if (_playerButton == null)
            Debug.LogWarning("_playerButton button not found");
        if (_eventName == null)
            Debug.LogWarning("_eventName button not found");
        if (_eventDate == null)
            Debug.LogWarning("_eventDate button not found");

        _pauseButton.RegisterCallback<ClickEvent>(SwitchToPauseState);
        _closeContextualPanelButton.RegisterCallback<ClickEvent>(CloseContextualPanel);
        _playerButton.RegisterCallback<ClickEvent>(SwitchToPlayerHUDState);
        _eventInfoButton.RegisterCallback<ClickEvent>(ShowEventInfo);
    }

    public override void StartState()
    {
        _screen.style.display = DisplayStyle.Flex; // Show HUD

        _eventName.text = ProgressManager.Instance._currentEvent.Name;
        _eventDate.text = ProgressManager.Instance._currentEvent.Date;

        HideTooltip();
    }

    public override void UpdateState()
    {
        _cursorScreenPos = Mouse.current.position.ReadValue();

        if (_tooltip != null &&
            _tooltip.style.display == DisplayStyle.Flex)
        {
            // UI Toolkit's Y axis is from top to bottom, while screen coordinates are from bottom to top
            _tooltip.style.left = _cursorScreenPos.x + UIManager.Instance._tooltipOffset.x; ;
            _tooltip.style.top = Screen.height - _cursorScreenPos.y + UIManager.Instance._tooltipOffset.y;
        }

        //UpdatePlayerButton();
    }

    public override void ExitState()
    {
        _screen.style.display = DisplayStyle.None; // Hide HUD
    }

    #endregion

    #region PUBLIC METHODS
    public void ShowTooltip(SelectableObject objectHovered)
    {
        if (_tooltip == null) return;

        // Overwrite and show tooltip
        _tooltip.text = objectHovered._information.Name;
        _tooltip.style.display = DisplayStyle.Flex;
    }

    public void HideTooltip()
    {
        if (_tooltip == null) return;

        _tooltip.style.display = DisplayStyle.None;
    }

    public void ShowContextualPanel(SelectableObject objectSelected)
    {
        if (IsCursorOverUI(_cursorScreenPos)) return;
        if (_contextualPanel == null) return;

        // Overwrite panel information
        _contextualPanelName.text = objectSelected._information.Name;
        _contextualPanelDescription.text = objectSelected._information.Description;

        // Show panel
        _contextualPanel.style.display = DisplayStyle.Flex;
    }

    public void HideContextualPanel()
    {
        if (_contextualPanel == null) return;

        // Hide panel
        _contextualPanel.style.display = DisplayStyle.None;

        // Switch to spectator camera state if it's not current state
        if (!CameraManager.Instance._fsm.IsCurrentState(CameraManager.Instance._spectatorState))
            CameraManager.Instance.SwitchToSpectatorCamera();

        CameraManager.Instance.ResetContextualPanelOffset();
    }
    #endregion

    #region PRIVATE METHODS
    void SwitchToPauseState(ClickEvent evt)
    {
        _stateMachine.SwitchState(UIManager.Instance._pauseState);
    }

    void CloseContextualPanel(ClickEvent evt)
    {
        HideContextualPanel();
    }

    void SwitchToPlayerHUDState(ClickEvent evt)
    {
        if (_playerButton == null) return;

        //_playerButton.style.display = DisplayStyle.None;
        _stateMachine.SwitchState(UIManager.Instance._playerHUDState);
        CameraManager.Instance.ToggleCameraState();
    }


    private void ShowEventInfo(ClickEvent evt)
    {
        CameraManager.Instance.ApplyContextualPanelOffset();

        // Overwrite panel information
        _contextualPanelName.text = ProgressManager.Instance._currentEvent.Name;
        _contextualPanelDescription.text = ProgressManager.Instance._currentEvent.Description;

        // Show panel
        _contextualPanel.style.display = DisplayStyle.Flex;
    }

    // TODO: DEPRECATED BECAUSE UGUI SOLUTION IS MORE FRAME RESPONSIVE
    // void UpdatePlayerButton()
    // {
    //     if (_playerButton == null) return;

    //     // Get the player's world position and an offset above the head
    //     Vector3 playerWorldPos = GameObject.FindGameObjectWithTag("Player").transform.position + Vector3.up * 2.0f;

    //     // Convert world position to screen position
    //     Vector3 playerScreenPos = Camera.main.WorldToScreenPoint(playerWorldPos);

    //     // Convert screen position to UI Toolkit coordinates
    //     // UI Toolkit's y=0 is at the top, but ScreenPoint's y=0 is at the bottom, so invert y
    //     _playerButton.style.left = playerScreenPos.x + UIManager.Instance._playerButtonOffset.x;
    //     _playerButton.style.top = Screen.height - playerScreenPos.y + UIManager.Instance._playerButtonOffset.y;

    //     //Hide the button if the player is off - screen
    //     _playerButton.style.display = (playerScreenPos.z > 0 &&
    //                                     playerScreenPos.x >= 0 && playerScreenPos.x <= Screen.width &&
    //                                     playerScreenPos.y >= 0 && playerScreenPos.y <= Screen.height)
    //                                     ? DisplayStyle.Flex
    //                                     : DisplayStyle.None;
    // }
    #endregion
}
