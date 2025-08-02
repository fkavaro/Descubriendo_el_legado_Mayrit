using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using System;

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
        _milestoneInfoButton,
        _playerButton,
        _nextMilestoneButton,
        _previousMilestoneButton,
        _playCharacterButton;
    VisualElement _contextualPanel,
        _milestoneArea,
        _contextualPanelImage;
    Vector2 _cursorScreenPos;
    #endregion

    #region INHERITED
    public SpectatorHUD_UIState(StackFiniteStateMachine<UIManager> stateMachine)
    : base("SpectatorHUD", stateMachine) { }

    public override void AwakeState()
    {
        _UIDocument = UIManager.Instance._UIDocument;
        _pauseButton = _UIDocument.rootVisualElement.Q<Button>("PauseButton");
        _playerButton = _UIDocument.rootVisualElement.Q<Button>("PlayerButton");

        _screen = _UIDocument.rootVisualElement.Q<VisualElement>("SpectatorHUD");
        _tooltip = _screen.Q<Label>("Tooltip");
        _milestoneArea = _screen.Q<VisualElement>("MilestoneArea");
        _milestoneInfoButton = _milestoneArea.Q<Button>("InfoButton");
        _milestoneName = _milestoneArea.Q<Label>("Name");
        _milestoneDate = _milestoneArea.Q<Label>("Date");
        _nextMilestoneButton = _milestoneArea.Q<Button>("NextMilestoneButton");
        _previousMilestoneButton = _milestoneArea.Q<Button>("PreviousMilestoneButton");
        _contextualPanel = _screen.Q<VisualElement>("ContextualPanel");
        _contextualPanelName = _contextualPanel.Q<Label>("Name");
        _contextualPanelDescription = _contextualPanel.Q<Label>("Description");
        _closeContextualPanelButton = _contextualPanel.Q<Button>("CloseButton");
        _contextualPanelImage = _contextualPanel.Q<VisualElement>("Image");
        _contextualPanelCaption = _contextualPanel.Q<Label>("Caption");
        _playCharacterButton = _contextualPanel.Q<Button>("PlayCharacterButton");

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
        if (_milestoneInfoButton == null)
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
        if (_nextMilestoneButton == null)
            Debug.LogWarning("_nextMilestoneButton button not found");
        if (_previousMilestoneButton == null)
            Debug.LogWarning("_previousMilestoneButton button not found");
        if (_playCharacterButton == null)
            Debug.LogWarning("_playCharacterButton button not found");

        _pauseButton.RegisterCallback<ClickEvent>(SwitchToPauseState);
        _closeContextualPanelButton.RegisterCallback<ClickEvent>(CloseContextualPanel);
        _playerButton.RegisterCallback<ClickEvent>(SwitchToPlayerHUDState);
        _milestoneInfoButton.RegisterCallback<ClickEvent>(ShowMilestoneInfo);
        _nextMilestoneButton.RegisterCallback<ClickEvent>(SwitchToNextMilestone);
        _previousMilestoneButton.RegisterCallback<ClickEvent>(SwitchToPreviousMilestone);
        _playCharacterButton.RegisterCallback<ClickEvent>(PlayCharacter);
    }

    public override void StartState()
    {
        _screen.style.display = DisplayStyle.Flex; // Show HUD

        CheckMilestoneButtonsActivation();
        HideContextualPanel();
        HideTooltip();
        OverwriteMilestoneArea();

        var previousState = _stateMachine.GetPreviousState();

        if (previousState == null)
            ShowMilestoneInfo();
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

    public void ShowContextualPanel(SelectableObject objectInfo)
    {
        if (IsCursorOverUI(_cursorScreenPos)) return;
        if (_contextualPanel == null) return;

        _milestoneArea.style.display = DisplayStyle.None;

        // Overwrite panel information
        _contextualPanelName.text = objectInfo._information.Name;
        _contextualPanelDescription.text = objectInfo._information.Description;

        // There is an image
        if (objectInfo._information.Image != null)
        {
            _contextualPanelImage.style.backgroundImage = new StyleBackground(objectInfo._information.Image.texture);
            _contextualPanelImage.style.display = DisplayStyle.Flex;
            _contextualPanelCaption.text = objectInfo._information.ImageCaption;
            _contextualPanelCaption.style.display = DisplayStyle.Flex;
        }

        // Show panel
        _contextualPanel.style.display = DisplayStyle.Flex;
    }

    public void ShowContextualPanel(CharacterInformationSO characterInfo)
    {
        _milestoneArea.style.display = DisplayStyle.None;

        // Overwrite panel information
        _contextualPanelName.text = characterInfo.Name;
        _contextualPanelDescription.text = characterInfo.Description;

        // There is an image
        if (characterInfo.Image != null)
        {
            _contextualPanelImage.style.backgroundImage = new StyleBackground(characterInfo.Image.texture);
            _contextualPanelImage.style.display = DisplayStyle.Flex;
            _contextualPanelCaption.text = characterInfo.ImageCaption;
            _contextualPanelCaption.style.display = DisplayStyle.Flex;
        }

        // Show play character button
        _playCharacterButton.style.display = DisplayStyle.Flex;

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
        _playCharacterButton.style.display = DisplayStyle.None;

        // Hide panel
        _contextualPanel.style.display = DisplayStyle.None;

        // Switch to spectator camera state if it's not current state
        if (!CameraManager.Instance._fsm.IsCurrentState(CameraManager.Instance._spectatorState))
            CameraManager.Instance.SwitchToSpectatorCamera();

        CameraManager.Instance.ResetContextualPanelOffset();
    }
    #endregion

    #region PRIVATE METHODS
    void OverwriteMilestoneArea()
    {
        _milestoneName.text = ProgressManager.Instance._currentMilestone.informationSO.Name;
        _milestoneDate.text = ProgressManager.Instance._currentMilestone.informationSO.Date;
    }

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


    void ShowMilestoneInfo(ClickEvent evt)
    {
        ShowMilestoneInfo();
    }

    void SwitchToPreviousMilestone(ClickEvent evt)
    {
        ProgressManager.Instance.SwitchToPreviousMilestone();
        OverwriteMilestoneArea();
        CheckMilestoneButtonsActivation();
        ShowMilestoneInfo();
    }

    void SwitchToNextMilestone(ClickEvent evt)
    {
        ProgressManager.Instance.SwitchToNextMilestone();
        OverwriteMilestoneArea();
        CheckMilestoneButtonsActivation();
        ShowMilestoneInfo();
    }

    void CheckMilestoneButtonsActivation()
    {
        // Last milestone
        if (ProgressManager.Instance.AtLastMilestone())
        {
            // Disable next button
            _nextMilestoneButton.SetEnabled(false);
        }
        else
            _nextMilestoneButton.SetEnabled(true);

        // First milestone
        if (ProgressManager.Instance.AtFirstMilestone())
        {
            // Disable previous button
            _previousMilestoneButton.SetEnabled(false);
        }
        else
            _previousMilestoneButton.SetEnabled(true);
    }

    void ShowMilestoneInfo()
    {
        // TODO: test experience this gives
        //CameraManager.Instance.ApplyContextualPanelOffset();

        _milestoneArea.style.display = DisplayStyle.None;

        // Overwrite panel information
        _contextualPanelName.text = ProgressManager.Instance._currentMilestone.informationSO.Name;
        _contextualPanelDescription.text = ProgressManager.Instance._currentMilestone.informationSO.Description;

        // Show panel
        _contextualPanel.style.display = DisplayStyle.Flex;
    }

    void PlayCharacter(ClickEvent evt)
    {
        CameraManager.Instance.ToggleCameraState();
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
