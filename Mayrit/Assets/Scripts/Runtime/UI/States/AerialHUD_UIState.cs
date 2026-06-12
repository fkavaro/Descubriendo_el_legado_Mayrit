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
        _previousMilestoneButton;
    VisualElement _milestoneArea,
        _playerFollowerRoot,
        _switches,
        _nextMilestoneButtonImage;

    Switch _modernVisualizactionSwitch;

    public Action PauseClickedEvent;
    public Action MilestoneInfoClickedEvent;
    public Action PreviousMilestoneClickedEvent;
    public Action NextMilestoneClickedEvent;
    public Action<bool> ModernVisualizationToggled;

    ProgressSystem _progressSystem;
    #endregion

    #region CONSTRUCTOR
    public AerialHUD_UIState(UISystem uiSystem, UIDocument uiDocument, float fadeInDuration, float fadeOutDuration)
    : base(uiSystem, "AerialHUD", uiDocument, fadeInDuration, fadeOutDuration) { }
    #endregion

    #region UI STATE INHERITED METHODS
    protected override void ConfigureUIElementsOnAwake()
    {
        base.ConfigureUIElementsOnAwake();

        _pauseButton = GetButtonAndRegisterCallback("PauseButton", OnPauseClicked);
        _milestoneArea = GetByName<VisualElement>("MilestoneArea");
        _milestoneInfoButton = GetButtonAndRegisterCallback("Info", OnMilestoneClicked, _milestoneArea);
        _previousMilestoneButton = GetButtonAndRegisterCallback("Previous", OnPreviousMilestoneClicked, _milestoneArea);
        _nextMilestoneButtonImage = GetByName<VisualElement>("RightArrow", _nextMilestoneButton);
        _nextMilestoneButton = GetButtonAndRegisterCallback("Next", OnNextMilestoneClicked, _milestoneArea);
        _milestoneName = GetByName<Label>("Name", _milestoneArea);
        _milestoneDate = GetByName<Label>("Date", _milestoneArea);
        _playerFollowerRoot = GetByName<VisualElement>("PlayerFollower");
        _switches = GetByName<VisualElement>("Switches");
        _modernVisualizactionSwitch = GetSwitchAndRegisterCallback("ModernVisualizationSwitch", OnModernSuperpositionToggled, _switches);

        _playerFollower = new PlayerFollower(_playerFollowerRoot);
    }

    protected override void GetServicesDependenciesOnStart()
    {
        base.GetServicesDependenciesOnStart();

        _progressSystem = ServiceLocator.Instance.Get<ProgressSystem>();
    }

    protected override void SubscribeToServicesEventsOnStart()
    {
        base.SubscribeToServicesEventsOnStart();
        _gameManager.MilestoneChangedEvent += OnMilestoneChanged;
    }

    public override void StartState()
    {
        base.StartState();

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
        _gameManager.MilestoneChangedEvent -= OnMilestoneChanged;
    }
    #endregion

    #region PRIVATE METHODS
    void CheckMilestoneButtonsAvailability()
    {
        bool isTherePreviousMilestone = !_progressSystem.AtFirstMilestone();

        _previousMilestoneButton.SetEnabled(isTherePreviousMilestone);
        _previousMilestoneButton.pickingMode = isTherePreviousMilestone ? PickingMode.Position : PickingMode.Ignore;

        bool isCurrentMilestoneCompleted = _progressSystem.IsCurrentMilestoneCompleted();
        bool atLastMilestone = _progressSystem.AtLastMilestone();
        bool isNextMilestoneAvailable = !atLastMilestone && isCurrentMilestoneCompleted;

        _nextMilestoneButton.SetEnabled(isNextMilestoneAvailable);
        _nextMilestoneButton.pickingMode = isNextMilestoneAvailable ? PickingMode.Position : PickingMode.Ignore;
        _nextMilestoneButtonImage.pickingMode = isNextMilestoneAvailable ? PickingMode.Position : PickingMode.Ignore;
    }
    #endregion

    #region CALLBACK METHODS
    void OnPauseClicked(ClickEvent evt)
    {
        _soundSystem.PlayButtonClickSFX();
        PauseClickedEvent?.Invoke();
    }

    void OnMilestoneClicked(ClickEvent evt)
    {
        _soundSystem.PlayButtonClickSFX();
        MilestoneInfoClickedEvent?.Invoke();
    }

    void OnPreviousMilestoneClicked(ClickEvent evt)
    {
        _soundSystem.PlayButtonClickSFX();
        PreviousMilestoneClickedEvent?.Invoke();
    }

    void OnNextMilestoneClicked(ClickEvent evt)
    {
        _soundSystem.PlayButtonClickSFX();
        NextMilestoneClickedEvent?.Invoke();
    }

    void OnModernSuperpositionToggled(bool newValue)
    {
        _soundSystem.PlayButtonClickSFX();
        ModernVisualizationToggled?.Invoke(newValue);
    }

    void OnMilestoneChanged(Milestone_DataSO mapping)
    {
        CheckMilestoneButtonsAvailability();
        _switches.style.display = DisplayStyle.Flex;

        // Overwrite milestone area
        _milestoneName.text = mapping.Header;
        _milestoneDate.text = mapping.SubHeader;

        _playerFollower.PlayerTransform = _gameManager.PlayableCharacter.transform;
    }
    #endregion
}
