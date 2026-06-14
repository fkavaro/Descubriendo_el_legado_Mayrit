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
        _nextMilestoneButtonImage;

    Switch _modernVisualizactionSwitch;

    public Action PauseClickedEvent;
    public Action MilestoneInfoClickedEvent;
    public Action PreviousMilestoneClickedEvent;
    public Action NextMilestoneClickedEvent;
    public Action<bool> ModernVisualizationToggled;

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
        _milestoneInfoButton = GetButtonAndRegisterCallback("MilestoneInfoButton", OnMilestoneClicked, _milestoneArea);
        _previousMilestoneButton = GetButtonAndRegisterCallback("PreviousMilestoneButton", OnPreviousMilestoneClicked, _milestoneArea);
        _nextMilestoneButtonImage = GetByName<VisualElement>("RightArrow", _nextMilestoneButton);
        _nextMilestoneButton = GetButtonAndRegisterCallback("NextMilestoneButton", OnNextMilestoneClicked, _milestoneArea);
        _milestoneName = GetByName<Label>("Name", _milestoneArea);
        _milestoneDate = GetByName<Label>("Date", _milestoneArea);
        _playerFollowerRoot = GetByName<VisualElement>("PlayerFollower");
        _modernVisualizactionSwitch = GetSwitchAndRegisterCallback("ModernVisualizationSwitch", OnModernSuperpositionToggled);

        _playerFollower = new PlayerFollower(_playerFollowerRoot);
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
    #endregion

    #region PRIVATE METHODS
    void CheckMilestoneButtonsAvailability()
    {
        bool isTherePreviousMilestone = !_gameManager.AtFirstMilestone();

        _previousMilestoneButton.SetEnabled(isTherePreviousMilestone);
        _previousMilestoneButton.pickingMode = isTherePreviousMilestone ? PickingMode.Position : PickingMode.Ignore;

        bool isCurrentMilestoneCompleted = _gameManager.IsCurrentMilestoneCompleted;
        bool atLastMilestone = _gameManager.AtLastMilestone();
        bool isNextMilestoneAvailable = !atLastMilestone && isCurrentMilestoneCompleted;

        _nextMilestoneButton.SetEnabled(isNextMilestoneAvailable);
        _nextMilestoneButton.pickingMode = isNextMilestoneAvailable ? PickingMode.Position : PickingMode.Ignore;
        _nextMilestoneButtonImage.pickingMode = isNextMilestoneAvailable ? PickingMode.Position : PickingMode.Ignore;
    }
    #endregion

    #region PUBLIC METHODS
    public void UpdateMilestoneData(Milestone_DataSO mapping)
    {
        _milestoneName.text = mapping.Header;
        _milestoneDate.text = mapping.SubHeader;

        _playerFollower.PlayerTransform = _gameManager.PlayableCharacter.transform;
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
    #endregion
}
