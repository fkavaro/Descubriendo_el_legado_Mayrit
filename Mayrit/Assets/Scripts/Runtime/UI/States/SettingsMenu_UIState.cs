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

    public event Action CloseClickedEvent;
    public event Action<bool> EdgeScrollingToggledEvent;
    public event Action<bool> ShowPOIsToggledEvent;
    public event Action<bool> ShowControlsToggledEvent;
    public event Action<float> MusicVolumeChangedEvent;
    public event Action<float> SFXVolumeChangedEvent;
    #endregion

    #region CONSTRUCTOR
    public SettingsMenu_UIState(UISystem uiSystem, UIDocument uiDocument, float fadeInDuration, float fadeOutDuration)
    : base(uiSystem, "SettingsMenu", uiDocument, fadeInDuration, fadeOutDuration) { }
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

        _musicVolumeSlider.value = _soundSystem.MusicVolume;
        _sfxVolumeSlider.value = _soundSystem.EffectsVolume;
        _showControlsSwitch.Value = _gameManager.ControlsVisibilityValueSet;
        _edgeScrollingSwitch.Value = _gameManager.EdgeScrollingValueSet;
        _tutorialSettings.style.display = GameSaveSystem.LoadTutorialCompletion() ? DisplayStyle.Flex : DisplayStyle.None;
    }
    #endregion

    #region CALLBACK METHODS
    void OnCloseClicked(ClickEvent evt)
    {
        base.ExitState();
        _soundSystem.PlayButtonClickSFX();
        CloseClickedEvent?.Invoke();
    }

    void OnEdgeScrollingToggled(bool newValue)
    {
        _soundSystem.PlayButtonClickSFX();
        EdgeScrollingToggledEvent?.Invoke(newValue);
    }

    void OnShowPOIsToggled(bool newValue)
    {
        _soundSystem.PlayButtonClickSFX();
        ShowPOIsToggledEvent?.Invoke(newValue);
    }

    void OnShowControlsToggled(bool newValue)
    {
        _soundSystem.PlayButtonClickSFX();
        ShowControlsToggledEvent?.Invoke(newValue);
    }

    void OnMusicVolumeChanged(ChangeEvent<float> evt)
    {
        MusicVolumeChangedEvent?.Invoke(evt.newValue);
    }

    void OnSFXVolumeChanged(ChangeEvent<float> evt)
    {
        SFXVolumeChangedEvent?.Invoke(evt.newValue);
    }

    void OnResetTutorialClicked(ClickEvent evt)
    {
        GameSaveSystem.SaveTutorial(false);
        _tutorialSettings.style.display = DisplayStyle.None;
    }
    #endregion
}
