using System;
using UnityEngine;
using UnityEngine.UIElements;

public class SettingsMenu_UIState : AUIState
{
    #region PROPERTIES
    Button _closeButton,
        _resetTutorialButton;
    Switch _edgeScrollingSwitch,
    _showPOIsSwitch,
        _showControlsSwitch;
    Slider _musicVolumeSlider,
        _sfxVolumeSlider;

    VisualElement _tutorialSettings;
    #endregion

    #region CONSTRUCTOR
    public SettingsMenu_UIState(UIDocument uiDocument, float fadeInDuration, float fadeOutDuration)
    : base("SettingsMenu", uiDocument, fadeInDuration, fadeOutDuration) { }
    #endregion

    #region OVERRIDDEN METHODS
    protected override void ConfigureUIElementsOnAwake()
    {
        _closeButton = GetButtonAndRegisterCallback("CloseButton", OnCloseClicked);
        _edgeScrollingSwitch = GetSwitchAndRegisterCallback("EdgeScrollingSwitch", OnEdgeScrollingToggled);
        _showPOIsSwitch = GetSwitchAndRegisterCallback("ShowPOIsSwitch", OnShowPOIsToggled);
        _showControlsSwitch = GetSwitchAndRegisterCallback("ShowControlsSwitch", OnShowControlsToggled);
        _musicVolumeSlider = GetSliderAndRegisterCallback("MusicVolumeSlider", OnMusicVolumeChanged);
        _sfxVolumeSlider = GetSliderAndRegisterCallback("SFXVolumeSlider", OnSFXVolumeChanged);
        _tutorialSettings = GetByName<VisualElement>("TutorialSettings");
        _resetTutorialButton = GetButtonAndRegisterCallback("ResetTutorialButton", OnResetTutorialClicked, _tutorialSettings);
    }

    public override void StartState()
    {
        base.StartState();

        _musicVolumeSlider.value = _soundManager.MusicVolume;
        _sfxVolumeSlider.value = _soundManager.EffectsVolume;
        _showControlsSwitch.Value = _uiManager.ControlsVisibilityValueSet;
        _edgeScrollingSwitch.Value = _uiManager.EdgeScrollingValueSet;
        _tutorialSettings.style.display = GameSaveSystem.LoadTutorialCompletion() ? DisplayStyle.Flex : DisplayStyle.None;
    }
    #endregion

    #region CALLBACK METHODS
    void OnCloseClicked(ClickEvent evt)
    {
        base.ExitState();
        _soundManager.PlayButtonClickSFX();

        if (_gameManager.IsInMainMenuState)
            _uiManager.SwitchToMainMenuState();
        else if (_gameManager.IsInPauseState)
            _uiManager.SwitchToPauseState();
    }

    void OnEdgeScrollingToggled(bool newValue)
    {
        _uiManager.SetEdgeScrollingValue(newValue);
        _soundManager.PlayButtonClickSFX();
    }

    void OnShowPOIsToggled(bool newValue)
    {
        _uiManager.SetPOIsVisibility(newValue);
        _soundManager.PlayButtonClickSFX();
    }

    void OnShowControlsToggled(bool newValue)
    {
        _uiManager.SetControlsVisibility(newValue);
        _soundManager.PlayButtonClickSFX();
    }

    void OnMusicVolumeChanged(ChangeEvent<float> evt)
    {
        _uiManager.InvokeMusicVolumeChangedEvent(evt.newValue);
    }

    void OnSFXVolumeChanged(ChangeEvent<float> evt)
    {
        _uiManager.InvokeSFXVolumeChangedEvent(evt.newValue);
    }

    void OnResetTutorialClicked(ClickEvent evt)
    {
        GameSaveSystem.SaveTutorial(false);
        _tutorialSettings.style.display = DisplayStyle.None;
    }
    #endregion
}
