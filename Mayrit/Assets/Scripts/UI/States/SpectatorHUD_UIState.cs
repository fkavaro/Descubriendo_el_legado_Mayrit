using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

public class SpectatorHUD_UIState : AUIState
{
    #region PUBLIC PROPERTIES
    #endregion

    #region PRIVATE PROPERTIES
    Label _tooltip,
        _contextualPanelName,
        _contextualPanelDescription,
        _milestoneName,
        _milestoneDate,
        _contextualPanelCaption;
    Button _pauseButton,
        _closeContextualPanelButton,
        _eventInfoButton,
        _playerButton;
    VisualElement _contextualPanel,
        _milestoneArea,
        _contextualPanelImage;
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
        _milestoneArea = _screen.Q<VisualElement>("MilestoneArea");
        _eventInfoButton = _milestoneArea.Q<Button>("InfoButton");
        _milestoneName = _eventInfoButton.Q<Label>("Name");
        _milestoneDate = _eventInfoButton.Q<Label>("Date");
        _contextualPanel = _screen.Q<VisualElement>("ContextualPanel");
        _contextualPanelName = _contextualPanel.Q<Label>("Name");
        _contextualPanelDescription = _contextualPanel.Q<Label>("Description");
        _closeContextualPanelButton = _contextualPanel.Q<Button>("CloseButton");
        _contextualPanelImage = _contextualPanel.Q<VisualElement>("Image");
        _contextualPanelCaption = _contextualPanel.Q<Label>("Caption");

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
        if (_milestoneArea == null)
            Debug.LogWarning("_milestoneArea button not found");
        if (_playerButton == null)
            Debug.LogWarning("_playerButton button not found");
        if (_milestoneName == null)
            Debug.LogWarning("_milestoneName button not found");
        if (_milestoneDate == null)
            Debug.LogWarning("_milestoneDate button not found");
        if (_contextualPanelImage == null)
            Debug.LogWarning("_contextualPanelImage button not found");
        if (_contextualPanelCaption == null)
            Debug.LogWarning("_contextualPanelCaption button not found");

        _pauseButton.RegisterCallback<ClickEvent>(SwitchToPauseState);
        _closeContextualPanelButton.RegisterCallback<ClickEvent>(CloseContextualPanel);
        _playerButton.RegisterCallback<ClickEvent>(SwitchToPlayerHUDState);
        _eventInfoButton.RegisterCallback<ClickEvent>(ShowEventInfo);
    }

    public override void StartState()
    {
        _screen.style.display = DisplayStyle.Flex; // Show HUD

        Debug.Log($"ProgressManager.Instance: {ProgressManager.Instance}");
        Debug.Log($"_currentMilestone: {ProgressManager.Instance?._currentMilestone}");
        Debug.Log($"informationSO: {ProgressManager.Instance?._currentMilestone?.informationSO}");
        Debug.Log($"_eventName: {_milestoneName}");

        _milestoneName.text = ProgressManager.Instance._currentMilestone.informationSO.Name;
        _milestoneDate.text = ProgressManager.Instance._currentMilestone.informationSO.Date;

        HideContextualPanel();
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

        _milestoneArea.style.display = DisplayStyle.None;

        // Overwrite panel information
        _contextualPanelName.text = objectSelected._information.Name;
        _contextualPanelDescription.text = objectSelected._information.Description;

        if (objectSelected._information.Image != null)
        {
            _contextualPanelImage.style.backgroundImage = new StyleBackground(objectSelected._information.Image.texture);
            _contextualPanelImage.style.display = DisplayStyle.Flex;
        }

        _contextualPanelCaption.text = objectSelected._information.ImageCaption;
        _contextualPanelCaption.style.display = DisplayStyle.Flex;

        // Show panel
        _contextualPanel.style.display = DisplayStyle.Flex;
    }

    public void HideContextualPanel()
    {
        if (_contextualPanel == null) return;

        _milestoneArea.style.display = DisplayStyle.Flex;
        _contextualPanelImage.style.backgroundImage = null;
        _contextualPanelImage.style.display = DisplayStyle.None;
        _contextualPanelCaption.style.display = DisplayStyle.None;

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

        _milestoneArea.style.display = DisplayStyle.None;

        // Overwrite panel information
        _contextualPanelName.text = ProgressManager.Instance._currentMilestone.informationSO.Name;
        _contextualPanelDescription.text = ProgressManager.Instance._currentMilestone.informationSO.Description;

        // Show panel
        _contextualPanel.style.display = DisplayStyle.Flex;
    }

    // TODO: DEPRECATED BECAUSE UGUI SOLUTION IS MORE FRAME-RESPONSIVE
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
