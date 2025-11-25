using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

public class SpectatorHUD_UIState : AUIState
{
    #region PUBLIC PROPERTIES
    public event Action OnModernSuperpositionToggled;
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
        _previousMilestoneButton,
        _modernSuperpositionButton;
    VisualElement _milestoneArea,
        _contextualPanelRoot;
    Vector2 _cursorScreenPos;
    #endregion

    public SpectatorHUD_UIState(UIDocument uiDocument)
    : base("SpectatorHUD", uiDocument) { }

    #region INHERITED
    public override void StartState()
    {
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
        _modernSuperpositionButton = _screen.Q<Button>("ModernSuperpositionButton");

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
        if (_modernSuperpositionButton == null)
            Debug.LogWarning("_modernSuperpositionButton button not found");

        _contextualPanel = new(_contextualPanelRoot);

        _contextualPanel.OnClosePanel += OnContextualPanelClosed;
        _pauseButton.RegisterCallback<ClickEvent>(SwitchToPauseState);
        _heritageButton.RegisterCallback<ClickEvent>(SwitchToHeritageState);
        _milestoneInfoButton.RegisterCallback<ClickEvent>(ShowMilestoneInfo);
        _nextMilestoneButton.RegisterCallback<ClickEvent>(SwitchToNextMilestone);
        _previousMilestoneButton.RegisterCallback<ClickEvent>(SwitchToPreviousMilestone);
        _modernSuperpositionButton.RegisterCallback<ClickEvent>(evt => OnModernSuperpositionToggled?.Invoke());

        _screen.style.display = DisplayStyle.Flex; // Show HUD

        CheckMilestoneButtonsActivation();
        HideTooltip();
        OverwriteMilestoneArea();

        var previousState = UIManager.Instance.BehaviourSystem.GetPreviousState();

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

        // Hide tooltip if camera is not spectator
        if (!CameraManager.Instance.IsInSpectatorState)
            HideTooltip();
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

    public void ShowContextualPanel(AInformationSO objectInfo)
    {
        if (IsCursorOverUI()) return;

        _milestoneArea.style.display = DisplayStyle.None;

        _contextualPanel.ShowInfo(objectInfo);
    }

    public void OnContextualPanelClosed()
    {
        _milestoneArea.style.display = DisplayStyle.Flex;

        // Switch to spectator camera state if it's not current state
        if (CameraManager.Instance.IsInOrbitalState)
            CameraManager.Instance.SwitchToSpectatorCamera();
    }
    #endregion

    #region PRIVATE METHODS
    void ShowMilestoneInfo()
    {
        _milestoneArea.style.display = DisplayStyle.None;

        MilestoneState currentProgressState = ProgressManager.Instance.BehaviourSystem.CurrentState;

        _contextualPanel.ShowInfo(currentProgressState.MilestoneMapping.Data);
    }

    void OverwriteMilestoneArea()
    {
        MilestoneState currentProgressState = ProgressManager.Instance.BehaviourSystem.CurrentState;

        if (currentProgressState == null || currentProgressState.MilestoneMapping.Data == null)
        {
            Debug.LogWarning("Current progress state or its informationSO is null");
            return;
        }

        _milestoneName.text = currentProgressState.MilestoneMapping.Data.Header;
        _milestoneDate.text = currentProgressState.MilestoneMapping.Data.SubHeader;
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
        UIManager.Instance.BehaviourSystem.SwitchState(UIManager.Instance._pauseState);
    }

    void SwitchToHeritageState(ClickEvent evt)
    {
        UIManager.Instance.BehaviourSystem.SwitchState(UIManager.Instance._heritageState);
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
    #endregion
}
