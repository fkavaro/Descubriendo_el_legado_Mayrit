using System;
using UnityEngine;
using UnityEngine.UIElements;

public class SpectatorHUD_UIState : AHUDState
{
    #region PROPERTIES
    PlayerFollower _playerFollower;

    Label _milestoneName,
        _milestoneDate;
    Button _pauseButton,
        _milestoneInfoButton,
        _nextMilestoneButton,
        _previousMilestoneButton;
    VisualElement _milestoneArea,
        _playerFollowerRoot,
        _switches,
        _milestoneButtons,
        _nextMilestoneButtonImage;

    public Switch _modernVisualizactionSwitch,
        _landmarkVisualizationSwitch;
    #endregion

    #region CONSTRUCTOR
    public SpectatorHUD_UIState(UIDocument uiDocument, float fadeInDuration, float fadeOutDuration)
    : base("SpectatorHUD", uiDocument, fadeInDuration, fadeOutDuration) { }
    #endregion

    #region UI STATE INHERITED METHODS
    protected override void ConfigureUIElementsOnAwake()
    {
        base.ConfigureUIElementsOnAwake();

        _pauseButton = GetButtonAndRegisterCallback("PauseButton", OnPauseClicked);
        _milestoneArea = GetByName<VisualElement>("MilestoneArea");
        _milestoneInfoButton = GetButtonAndRegisterCallback("InfoButton", OnMilestoneClicked, _milestoneArea);
        _milestoneName = GetByName<Label>("Name", _milestoneArea);
        _milestoneDate = GetByName<Label>("Date", _milestoneArea);
        _playerFollowerRoot = GetByName<VisualElement>("PlayerFollower");
        _switches = GetByName<VisualElement>("Switches");
        _modernVisualizactionSwitch = GetSwitchAndRegisterCallback("ModernVisualizationSwitch", OnModernSuperpositionToggled, _switches);
        _landmarkVisualizationSwitch = GetSwitchAndRegisterCallback("LandmarkVisualizationSwitch", OnLandmarkVisualizationToggled, _switches);
        _milestoneButtons = GetByName<VisualElement>("MilestoneButtons");
        _nextMilestoneButton = GetButtonAndRegisterCallback("NextMilestoneButton", OnNextMilestoneClicked, _milestoneButtons);
        _previousMilestoneButton = GetButtonAndRegisterCallback("PreviousMilestoneButton", OnPreviousMilestoneClicked, _milestoneButtons);
        _nextMilestoneButtonImage = GetByName<VisualElement>("RightArrow", _nextMilestoneButton);

        _playerFollower = new PlayerFollower(_playerFollowerRoot);
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
    }

    public override void StartState()
    {
        base.StartState();

        _switches.style.display = _wasContextualPanelShown ? DisplayStyle.None : DisplayStyle.Flex;
        _milestoneArea.style.display = _wasContextualPanelShown ? DisplayStyle.None : DisplayStyle.Flex;
        _milestoneButtons.style.display = _wasContextualPanelShown ? DisplayStyle.None : DisplayStyle.Flex;
        CheckMilestoneButtonsAvailability();
        _playerFollower.Start();
    }

    public override void UpdateState()
    {
        base.UpdateState();

        _playerFollower.Update();
    }

    protected override void UnsubscribeToServicesEventsOnExit()
    {
        base.UnsubscribeToServicesEventsOnExit();
        _progressManager.MilestoneChangedEvent -= OnMilestoneChanged;
    }
    #endregion

    #region HUD STATE INHERITED METHODS
    protected override void OnContextualPanelShown()
    {
        _switches.style.display = DisplayStyle.None;
        _milestoneButtons.style.display = DisplayStyle.None;
        _milestoneArea.style.display = DisplayStyle.None;
    }

    protected override void OnContextualPanelHidden()
    {
        _switches.style.display = DisplayStyle.Flex;
        _milestoneButtons.style.display = DisplayStyle.Flex;
        _milestoneArea.style.display = DisplayStyle.Flex;
    }
    #endregion

    #region PRIVATE METHODS
    void CheckMilestoneButtonsAvailability()
    {
        bool isNextMilestoneAvailable = _progressManager.IsNextMilestoneAvailable();
        bool isPreviousMilestoneAvailable = !_progressManager.AtFirstMilestone();

        _nextMilestoneButton.SetEnabled(isNextMilestoneAvailable);
        _nextMilestoneButton.pickingMode = isNextMilestoneAvailable ? PickingMode.Position : PickingMode.Ignore;
        _nextMilestoneButtonImage.pickingMode = isNextMilestoneAvailable ? PickingMode.Position : PickingMode.Ignore;

        _previousMilestoneButton.SetEnabled(isPreviousMilestoneAvailable);
        _previousMilestoneButton.pickingMode = isPreviousMilestoneAvailable ? PickingMode.Position : PickingMode.Ignore;
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

    // TODO: move to uiManager
    void OnModernSuperpositionToggled(bool newValue)
    {
        _soundManager.PlayButtonClickSFX();
    }
    void OnLandmarkVisualizationToggled(bool newValue)
    {
        _soundManager.PlayButtonClickSFX();
    }

    void OnMilestoneChanged(Milestone_DataSO mapping)
    {
        CheckMilestoneButtonsAvailability();
        _switches.style.display = DisplayStyle.Flex;

        // Overwrite milestone area
        _milestoneName.text = mapping.Header;
        _milestoneDate.text = mapping.SubHeader;

        _playerFollower.PlayerTransform = ServiceLocator.Instance.Get<PlayableCharacter>().transform;
    }
    #endregion
}
