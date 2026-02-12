using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

public class SpectatorHUD_UIState : AHUDState
{
    #region PROPERTIES
    public event Action OnModernSuperpositionEvent;

    PlayerFollower _playerFollowerComponent;

    Label _tooltip,
        _milestoneName,
        _milestoneDate;
    Button _pauseButton,
        _milestoneInfoButton,
        _nextMilestoneButton,
        _previousMilestoneButton,
        _modernSuperpositionButton;
    VisualElement _milestoneArea,
        _playerFollower;

    // Dependency Injection
    ProgressManager _progressManager;
    #endregion

    #region CONSTRUCTOR
    public SpectatorHUD_UIState(UIDocument uiDocument)
    : base("SpectatorHUD", uiDocument) { }
    #endregion

    #region UI STATE INHERITED METHODS
    protected override void ConfigureUIElementsOnAwake()
    {
        base.ConfigureUIElementsOnAwake();

        _pauseButton = _screen.Q<Button>("PauseButton");
        _tooltip = _screen.Q<Label>("Tooltip");
        _milestoneArea = _screen.Q<VisualElement>("MilestoneArea");
        _milestoneInfoButton = _milestoneArea.Q<Button>("InfoButton");
        _milestoneName = _milestoneArea.Q<Label>("Name");
        _milestoneDate = _milestoneArea.Q<Label>("Date");
        _nextMilestoneButton = _milestoneArea.Q<Button>("NextMilestoneButton");
        _previousMilestoneButton = _milestoneArea.Q<Button>("PreviousMilestoneButton");
        _modernSuperpositionButton = _screen.Q<Button>("ModernSuperpositionButton");
        _playerFollower = _screen.Q<VisualElement>("PlayerFollower");

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
        if (_playerFollower == null)
            Debug.LogWarning("_playerFollower not found");

        _playerFollowerComponent = new PlayerFollower(_playerFollower);
    }

    protected override void RegisterUICallbacksOnAwake()
    {
        _pauseButton.RegisterCallback<ClickEvent>(OnPauseClicked);
        _milestoneInfoButton.RegisterCallback<ClickEvent>(OnMilestoneClicked);
        _nextMilestoneButton.RegisterCallback<ClickEvent>(OnNextMilestoneClicked);
        _previousMilestoneButton.RegisterCallback<ClickEvent>(OnPreviousMilestoneClicked);
        _modernSuperpositionButton.RegisterCallback<ClickEvent>(OnModerSuperpositionToggled);
    }

    protected override void GetServicesDependenciesOnStart()
    {
        base.GetServicesDependenciesOnStart();

        _progressManager = ServiceLocator.Instance.Get<ProgressManager>();
    }

    protected override void SubscribeToServicesEventsOnStart()
    {
        base.SubscribeToServicesEventsOnStart();
        _progressManager.MilestoneChangedEvent += OnMilestoneChanged;

        // TODO: remove later
        //_uiManager.ShowTooltipEvent += OnShowTooltip;
        //_uiManager.HideTooltipEvent += OnHideTooltip;
    }

    public override void StartState()
    {
        base.StartState();

        if (_wasContextualPanelShown)
            _milestoneArea.style.display = DisplayStyle.None;
        else
            _milestoneArea.style.display = DisplayStyle.Flex;

        _nextMilestoneButton.SetEnabled(_progressManager.IsNextMilestoneAvailable());

        _playerFollowerComponent.Start();
    }

    public override void UpdateState()
    {
        base.UpdateState();

        _playerFollowerComponent.Update();
    }

    protected override void UnsubscribeToServicesEventsOnExit()
    {
        base.UnsubscribeToServicesEventsOnExit();
        _progressManager.MilestoneChangedEvent -= OnMilestoneChanged;

        // TODO: remove later
        //_uiManager.ShowTooltipEvent -= OnShowTooltip;
        //_uiManager.HideTooltipEvent -= OnHideTooltip;
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
    void OnMilestoneClicked(ClickEvent evt)
    {
        ShowContextualPanel(_progressManager.CurrentMilestoneMapping);
        _soundManager.PlayButtonClickSFX();
    }

    void OnPreviousMilestoneClicked(ClickEvent evt)
    {
        _progressManager.SwitchToPreviousMilestone();
        _soundManager.PlayButtonClickSFX();
    }

    void OnNextMilestoneClicked(ClickEvent evt)
    {
        _progressManager.SwitchToNextMilestone();
        _soundManager.PlayButtonClickSFX();
    }

    void OnModerSuperpositionToggled(ClickEvent evt)
    {
        OnModernSuperpositionEvent?.Invoke();
        _soundManager.PlayButtonClickSFX();
    }

    void OnMilestoneChanged(Milestone_DataSO mapping)
    {
        // Overwrite milestone area
        _milestoneName.text = mapping.Header;
        _milestoneDate.text = mapping.SubHeader;

        _nextMilestoneButton.SetEnabled(_progressManager.IsNextMilestoneAvailable());

        // First milestone
        if (_progressManager.AtFirstMilestone())
            // Disable previous button
            _previousMilestoneButton.SetEnabled(false);
        else
            _previousMilestoneButton.SetEnabled(true);

        ShowContextualPanel(mapping);

        _playerFollowerComponent.PlayerTransform = ServiceLocator.Instance.Get<PlayableCharacter>().transform;
    }

    // TODO: remove later
    // void OnShowTooltip(DataSO data)
    // {
    //     if (!_cameraManager.IsInSpectatorState || data == null)
    //     {
    //         OnHideTooltip();
    //         return;
    //     }

    //     if (_tooltip.text != data.Header)
    //         _tooltip.text = data.Header;
    //     if (_tooltip.style.display != DisplayStyle.Flex)
    //         _tooltip.style.display = DisplayStyle.Flex;
    //     Vector2 _cursorScreenPos = Mouse.current.position.ReadValue();
    //     // UI Toolkit's Y axis is from top to bottom, 
    //     // while screen coordinates are from bottom to top
    //     _tooltip.style.left = _cursorScreenPos.x + _uiManager.TooltipOffset.x; ;
    //     _tooltip.style.top = Screen.height - _cursorScreenPos.y + _uiManager.TooltipOffset.y;
    // }
    // void OnHideTooltip()
    // {
    //     if (_tooltip.style.display != DisplayStyle.None)
    //         _tooltip.style.display = DisplayStyle.None;
    // }
    #endregion
}
