using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

public class SpectatorHUD_UIState : AHUDState
{
    #region PROPERTIES
    PlayerFollower _playerFollowerComponent;

    Label _tooltip,
        _milestoneName,
        _milestoneDate;
    Button _pauseButton,
        _milestoneInfoButton,
        _nextMilestoneButton,
        _previousMilestoneButton;
    VisualElement _milestoneArea,
        _playerFollower,
        _switches;

    public Switch _modernVisualizactionSwitch,
        _landmarkVisualizationSwitch;
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
        _playerFollower = _screen.Q<VisualElement>("PlayerFollower");
        _switches = _screen.Q<VisualElement>("Switches");
        _modernVisualizactionSwitch = _switches.Q<Switch>("ModernVisualizationSwitch");
        _landmarkVisualizationSwitch = _switches.Q<Switch>("LandmarkVisualizationSwitch");

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
        if (_playerFollower == null)
            Debug.LogWarning("_playerFollower not found");
        if (_switches == null)
            Debug.LogWarning("_switches not found");
        if (_modernVisualizactionSwitch == null)
            Debug.LogWarning("_modernVisualizactionSwitch not found");
        if (_landmarkVisualizationSwitch == null)
            Debug.LogWarning("_landmarkVisualizationSwitch not found");

        _playerFollowerComponent = new PlayerFollower(_playerFollower);
    }

    protected override void RegisterUICallbacksOnAwake()
    {
        _pauseButton.RegisterCallback<ClickEvent>(OnPauseClicked);
        _milestoneInfoButton.RegisterCallback<ClickEvent>(OnMilestoneClicked);
        _nextMilestoneButton.RegisterCallback<ClickEvent>(OnNextMilestoneClicked);
        _previousMilestoneButton.RegisterCallback<ClickEvent>(OnPreviousMilestoneClicked);

        _modernVisualizactionSwitch.Toggled += OnModernSuperpositionToggled;
        _landmarkVisualizationSwitch.Toggled += OnLandmarkVisualizationToggled;
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
        {
            _switches.style.display = DisplayStyle.Flex;
            _milestoneArea.style.display = DisplayStyle.Flex;
        }

        _nextMilestoneButton.SetEnabled(_progressManager.IsNextMilestoneAvailable());
        _nextMilestoneButton.pickingMode = _progressManager.IsNextMilestoneAvailable() ? PickingMode.Position : PickingMode.Ignore;

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
        _switches.style.display = DisplayStyle.None;
    }

    protected override void OnContextualPanelHidden()
    {
        _milestoneArea.style.display = DisplayStyle.Flex;
        _switches.style.display = DisplayStyle.Flex;
    }
    #endregion

    #region CALLBACK METHODS
    void OnMilestoneClicked(ClickEvent evt)
    {
        ShowContextualPanel(_progressManager.CurrentMilestoneData);
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

    private void OnModernSuperpositionToggled(bool value)
    {
        _soundManager.PlayButtonClickSFX();
    }
    private void OnLandmarkVisualizationToggled(bool value)
    {
        _soundManager.PlayButtonClickSFX();
    }

    void OnMilestoneChanged(Milestone_DataSO mapping)
    {
        _switches.style.display = DisplayStyle.Flex;

        // Overwrite milestone area
        _milestoneName.text = mapping.Header;
        _milestoneDate.text = mapping.SubHeader;

        _nextMilestoneButton.SetEnabled(_progressManager.IsNextMilestoneAvailable());
        _nextMilestoneButton.pickingMode = _progressManager.IsNextMilestoneAvailable() ? PickingMode.Position : PickingMode.Ignore;

        _previousMilestoneButton.SetEnabled(!_progressManager.AtFirstMilestone());
        _previousMilestoneButton.pickingMode = !_progressManager.AtFirstMilestone() ? PickingMode.Position : PickingMode.Ignore;

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
