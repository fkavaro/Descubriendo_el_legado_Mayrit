using System;
using UnityEngine;
using UnityEngine.UIElements;

public class AerialHUD_UIState : AHUDState
{
    #region PROPERTIES
    PlayerFollower _playerFollower;

    Label _milestoneName,
        _milestoneDate;
    Button _pauseButton,
        _milestoneInfoButton,
        _nextMilestoneButton,
        _previousMilestoneButton,
        _mainMenuButton;
    VisualElement _milestoneArea,
        _playerFollowerRoot,
        _switches,
        _milestoneButtons,
        _nextMilestoneButtonImage;

    public Switch _modernVisualizactionSwitch;
    #endregion

    #region CONSTRUCTOR
    public AerialHUD_UIState(UIDocument uiDocument, float fadeInDuration, float fadeOutDuration)
    : base("AerialHUD", uiDocument, fadeInDuration, fadeOutDuration) { }
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
        _milestoneButtons = GetByName<VisualElement>("MilestoneButtons");
        _previousMilestoneButton = GetButtonAndRegisterCallback("PreviousMilestoneButton", OnPreviousMilestoneClicked, _milestoneButtons);
        _nextMilestoneButtonImage = GetByName<VisualElement>("RightArrow", _nextMilestoneButton);
        _nextMilestoneButton = GetButtonAndRegisterCallback("NextMilestoneButton", OnNextMilestoneClicked, _milestoneButtons);
        _mainMenuButton = GetButtonAndRegisterCallback("MainMenuButton", OnMainMenuClicked);

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
        bool isTherePreviousMilestone = !_progressManager.AtFirstMilestone();
        bool isCurrentMilestoneCompleted = _progressManager.IsCurrentMilestoneCompleted();
        bool atLastMilestone = _progressManager.AtLastMilestone();
        bool displayMainMenuButton = atLastMilestone && isCurrentMilestoneCompleted;

        _previousMilestoneButton.SetEnabled(isTherePreviousMilestone);
        _previousMilestoneButton.pickingMode = isTherePreviousMilestone ? PickingMode.Position : PickingMode.Ignore;

        _nextMilestoneButton.style.display = atLastMilestone ? DisplayStyle.None : DisplayStyle.Flex;
        _nextMilestoneButton.SetEnabled(isCurrentMilestoneCompleted);
        _nextMilestoneButton.pickingMode = isCurrentMilestoneCompleted ? PickingMode.Position : PickingMode.Ignore;
        _nextMilestoneButtonImage.pickingMode = isCurrentMilestoneCompleted ? PickingMode.Position : PickingMode.Ignore;

        _mainMenuButton.style.display = displayMainMenuButton ? DisplayStyle.Flex : DisplayStyle.None;
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

    void OnMainMenuClicked(ClickEvent evt)
    {
        _gameManager.SwitchToMainMenuState();
        _soundManager.PlayButtonClickSFX();
    }

    // TODO: move to uiManager
    void OnModernSuperpositionToggled(bool newValue)
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
