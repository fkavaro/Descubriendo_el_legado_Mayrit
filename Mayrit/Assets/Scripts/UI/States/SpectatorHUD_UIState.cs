using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

public class SpectatorHUD_UIState : AHUDState
{
    #region PROPERTIES
    public event Action OnModernSuperpositionEvent;

    Label _tooltip,
        _milestoneName,
        _milestoneDate;
    Button _pauseButton,
        _heritageButton,
        _milestoneInfoButton,
        _nextMilestoneButton,
        _previousMilestoneButton,
        _modernSuperpositionButton;
    VisualElement _milestoneArea;

    // Dependency Injection
    CameraManager _cameraManager;
    ProgressManager _progressManager;
    #endregion

    #region CONSTRUCTOR
    public SpectatorHUD_UIState(UIDocument uiDocument)
    : base("SpectatorHUD", uiDocument)
    {
        // Get dependencies from Service Locator
        _cameraManager = ServiceLocator.Instance.Get<CameraManager>();
        _progressManager = ServiceLocator.Instance.Get<ProgressManager>();

        // Validate dependencies
        if (_cameraManager == null)
            Debug.LogError("SpectatorHUD_UIState: CameraManager not found in ServiceLocator!");
        if (_progressManager == null)
            Debug.LogError("SpectatorHUD_UIState: ProgressManager not found in ServiceLocator!");
    }
    #endregion

    #region UI STATE INHERITED METHODS
    protected override void ConfigureUIElements()
    {
        _pauseButton = _screen.Q<Button>("PauseButton");
        _heritageButton = _screen.Q<Button>("HeritageButton");
        _tooltip = _screen.Q<Label>("Tooltip");
        _milestoneArea = _screen.Q<VisualElement>("MilestoneArea");
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
    }

    protected override void RegisterCallbacks()
    {
        _pauseButton.RegisterCallback<ClickEvent>(OnPauseClicked);
        _heritageButton.RegisterCallback<ClickEvent>(OnHeritageClicked);
        _milestoneInfoButton.RegisterCallback<ClickEvent>(OnMilestoneClicked);
        _nextMilestoneButton.RegisterCallback<ClickEvent>(OnNextMilestoneClicked);
        _previousMilestoneButton.RegisterCallback<ClickEvent>(OnPreviousMilestoneClicked);
        _modernSuperpositionButton.RegisterCallback<ClickEvent>(OnModerSuperpositionToggled);
        _contextualPanel.PlayCharacterClickedEvent += OnPlayCharacterClicked;

        _uiManager.ShowTooltipEvent += OnShowTooltip;
        _uiManager.HideTooltipEvent += OnHideTooltip;
        _progressManager.OnMilestoneChangedEvent += OnMilestoneChanged;
    }

    private void OnPlayCharacterClicked()
    {
        // Hide contextual panel
        HideContextualPanel();
    }

    protected override void OnStartState()
    {
        if (!_wasContextualPanelShown)
            _milestoneArea.style.display = DisplayStyle.Flex;
    }
    #endregion

    #region HUD STATE INHERITED METHODS
    protected override void OnContextualPanelShown()
    {
        _milestoneArea.style.display = DisplayStyle.None;
    }

    protected override void OnContextualPanelHidden()
    {
        _milestoneArea.style.display = DisplayStyle.Flex;
    }
    #endregion

    #region CALLBACK METHODS
    void OnPauseClicked(ClickEvent evt)
    {
        _uiManager.SwitchToPauseState();
    }

    void OnHeritageClicked(ClickEvent evt)
    {
        _uiManager.SwitchToHeritageState();
    }

    void OnMilestoneClicked(ClickEvent evt)
    {
        ShowContextualPanel(_progressManager.CurrentMilestoneMapping.Data);
    }

    void OnPreviousMilestoneClicked(ClickEvent evt)
    {
        _progressManager.SwitchToPreviousMilestone();
    }

    void OnNextMilestoneClicked(ClickEvent evt)
    {
        _progressManager.SwitchToNextMilestone();
    }

    void OnModerSuperpositionToggled(ClickEvent evt)
    {
        OnModernSuperpositionEvent?.Invoke();
    }

    void OnMilestoneChanged(MilestoneMapping mapping)
    {
        // Overwrite milestone area
        _milestoneName.text = mapping.Data.Header;
        _milestoneDate.text = mapping.Data.SubHeader;

        // Check progress for enabling/disabling buttons
        // Last milestone
        if (_progressManager.AtLastMilestone())
            // Disable next button
            _nextMilestoneButton.SetEnabled(false);
        else
            _nextMilestoneButton.SetEnabled(true);

        // First milestone
        if (_progressManager.AtFirstMilestone())
            // Disable previous button
            _previousMilestoneButton.SetEnabled(false);
        else
            _previousMilestoneButton.SetEnabled(true);

        ShowContextualPanel(mapping.Data);
    }

    void OnShowTooltip(DataSO data)
    {
        if (!_cameraManager.IsInSpectatorState)
        {
            OnHideTooltip();
            return;
        }

        _tooltip.text = data.Header;
        _tooltip.style.display = DisplayStyle.Flex;

        Vector2 _cursorScreenPos = Mouse.current.position.ReadValue();

        // UI Toolkit's Y axis is from top to bottom, 
        // while screen coordinates are from bottom to top
        _tooltip.style.left = _cursorScreenPos.x + _uiManager._tooltipOffset.x; ;
        _tooltip.style.top = Screen.height - _cursorScreenPos.y + _uiManager._tooltipOffset.y;
    }

    void OnHideTooltip()
    {
        _tooltip.style.display = DisplayStyle.None;
    }
    #endregion
}
