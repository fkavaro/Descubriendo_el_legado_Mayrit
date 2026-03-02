using System;
using UnityEngine;
using UnityEngine.UIElements;

public class SettingsMenu_UIState : AUIState
{
    #region PROPERTIES
    Button _closeButton;
    Switch _edgeScrollingSwitch,
        _showControlsSwitch;
    Slider _musicVolumeSlider,
        _sfxVolumeSlider;
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
        _showControlsSwitch = GetSwitchAndRegisterCallback("ShowControlsSwitch", OnShowControlsToggled);
        _musicVolumeSlider = GetSliderAndRegisterCallback("MusicVolumeSlider", OnMusicVolumeChanged);
        _sfxVolumeSlider = GetSliderAndRegisterCallback("SFXVolumeSlider", OnSFXVolumeChanged);
    }

    public override void StartState()
    {
        base.StartState();

        _musicVolumeSlider.value = _soundManager.MusicVolume;
        _sfxVolumeSlider.value = _soundManager.EffectsVolume;
        _showControlsSwitch.Value = _uiManager.ControlsVisibilityValueSet;
        _edgeScrollingSwitch.Value = _uiManager.EdgeScrollingValueSet;
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
        _uiManager.InvokeEdgeScrollingToggledEvent(newValue);
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
    #endregion
}
