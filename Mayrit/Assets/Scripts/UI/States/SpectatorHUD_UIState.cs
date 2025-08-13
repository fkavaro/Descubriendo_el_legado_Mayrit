using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

public class SpectatorHUD_UIState : AUIState
{
    #region PUBLIC PROPERTIES
    public ContextualPanel _contextualPanel;
    #endregion

    #region PRIVATE PROPERTIES
    Label _tooltip,
        _milestoneName,
        _milestoneDate;
    Button _pauseButton,
        _heritageButton,
        _milestoneInfoButton,
        _nextMilestoneButton,
        _previousMilestoneButton;
    VisualElement _milestoneArea,
        _contextualPanelRoot;
    Vector2 _cursorScreenPos;
    #endregion

    #region INHERITED
    public SpectatorHUD_UIState(StackFiniteStateMachine<UIManager> stateMachine)
    : base("SpectatorHUD", stateMachine) { }

    public override void AwakeState()
    {
        _UIDocument = UIManager.Instance._UIDocument;
        _screen = _UIDocument.rootVisualElement.Q<VisualElement>("SpectatorHUD");

        _pauseButton = _screen.Q<Button>("PauseButton");
        _heritageButton = _screen.Q<Button>("HeritageButton");
        _tooltip = _screen.Q<Label>("Tooltip");
        _milestoneArea = _screen.Q<VisualElement>("MilestoneArea");
        _contextualPanelRoot = _screen.Q<VisualElement>("ContextualPanel");
        _milestoneInfoButton = _milestoneArea.Q<Button>("InfoButton");
        _milestoneName = _milestoneArea.Q<Label>("Name");
        _milestoneDate = _milestoneArea.Q<Label>("Date");
        _nextMilestoneButton = _milestoneArea.Q<Button>("NextMilestoneButton");
        _previousMilestoneButton = _milestoneArea.Q<Button>("PreviousMilestoneButton");

        if (_pauseButton == null)
            Debug.LogWarning("_pauseButton not found");
        if (_tooltip == null)
            Debug.LogWarning("_tooltip not found");
        if (_milestoneArea == null)
            Debug.LogWarning("_milestoneArea not found");
        if (_contextualPanelRoot == null)
            Debug.LogWarning("_contextualPanel not found");
        if (_milestoneInfoButton == null)
            Debug.LogWarning("_eventInfoButton button not found");
        if (_milestoneName == null)
            Debug.LogWarning("_milestoneName not found");
        if (_milestoneDate == null)
            Debug.LogWarning("_milestoneDate not found");
        if (_nextMilestoneButton == null)
            Debug.LogWarning("_nextMilestoneButton button not found");
        if (_previousMilestoneButton == null)
            Debug.LogWarning("_previousMilestoneButton button not found");

        _contextualPanel = new(_contextualPanelRoot);

        _contextualPanel.OnClosePanel += OnContextualPanelClosed;
        _pauseButton.RegisterCallback<ClickEvent>(SwitchToPauseState);
        _heritageButton.RegisterCallback<ClickEvent>(SwitchToHeritageState);
        _milestoneInfoButton.RegisterCallback<ClickEvent>(ShowMilestoneInfo);
        _nextMilestoneButton.RegisterCallback<ClickEvent>(SwitchToNextMilestone);
        _previousMilestoneButton.RegisterCallback<ClickEvent>(SwitchToPreviousMilestone);
    }

    public override void StartState()
    {
        _screen.style.display = DisplayStyle.Flex; // Show HUD

        CheckMilestoneButtonsActivation();
        HideTooltip();
        OverwriteMilestoneArea();

        var previousState = _stateMachine.GetPreviousState();

        // To avoid showing the milestone info when switching from other state 
        // (PlayerHUD for example)
        if (previousState == null)
            ShowMilestoneInfo();
        else
            _contextualPanel.Hide();
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
        _tooltip.text = objectHovered._information.Header;
        _tooltip.style.display = DisplayStyle.Flex;
    }

    public void HideTooltip()
    {
        if (_tooltip == null) return;

        _tooltip.style.display = DisplayStyle.None;
    }

    public void ShowContextualPanel(InformationSO objectInfo)
    {
        if (IsCursorOverUI(_cursorScreenPos)) return;

        _milestoneArea.style.display = DisplayStyle.None;

        _contextualPanel.ShowInfo(objectInfo);
    }

    public void OnContextualPanelClosed()
    {
        _milestoneArea.style.display = DisplayStyle.Flex;

        // Switch to spectator camera state if it's not current state
        if (CameraManager.Instance._fsm.IsCurrentState(CameraManager.Instance._orbitalState))
            CameraManager.Instance.SwitchToSpectatorCamera();
    }
    #endregion

    #region PRIVATE METHODS
    void ShowMilestoneInfo()
    {
        _milestoneArea.style.display = DisplayStyle.None;

        AProgressState currentProgressState = (AProgressState)ProgressManager.Instance._fsm.CurrentState;

        _contextualPanel.ShowInfo(currentProgressState._informationSO);
    }

    void OverwriteMilestoneArea()
    {
        AProgressState currentProgressState = (AProgressState)ProgressManager.Instance._fsm.CurrentState;

        _milestoneName.text = currentProgressState._informationSO.Header;
        _milestoneDate.text = currentProgressState._informationSO.SubHeader;
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

    void SwitchToPauseState(ClickEvent evt)
    {
        _stateMachine.SwitchState(UIManager.Instance._pauseState);
    }

    void SwitchToHeritageState(ClickEvent evt)
    {
        _stateMachine.SwitchState(UIManager.Instance._heritageState);
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

    //! DEPRECATED BECAUSE UGUI SOLUTION IS MORE FRAME-RESPONSIVE
    // void SwitchToPlayerHUDState(ClickEvent evt)
    // {
    //     if (_playerButton == null) return;

    //     //_playerButton.style.display = DisplayStyle.None;
    //     _stateMachine.SwitchState(UIManager.Instance._playerHUDState);
    //     CameraManager.Instance.ToggleCameraState();
    // }

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
