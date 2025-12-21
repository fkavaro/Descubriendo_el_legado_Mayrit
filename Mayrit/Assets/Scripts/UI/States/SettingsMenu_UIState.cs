using System;
using UnityEngine;
using UnityEngine.UIElements;

public class SettingsMenu_UIState : AUIState
{
    #region PROPERTIES
    Button _closeButton;
    Toggle _edgeScrollingToggle,
        _showControlsToggle;
    Slider _musicVolumeSlider,
        _sfxVolumeSlider;
    #endregion

    #region CONSTRUCTOR
    public SettingsMenu_UIState(UIDocument uiDocument)
    : base("SettingsMenu", uiDocument) { }
    #endregion

    #region OVERRIDDEN METHODS
    protected override void ConfigureUIElementsOnAwake()
    {
        _closeButton = _screen.Q<Button>("CloseButton");
        _edgeScrollingToggle = _screen.Q<Toggle>("EdgeScrollingToggle");
        _showControlsToggle = _screen.Q<Toggle>("ShowControlsToggle");
        _musicVolumeSlider = _screen.Q<Slider>("MusicVolumeSlider");
        _sfxVolumeSlider = _screen.Q<Slider>("SFXVolumeSlider");

        if (_closeButton == null)
            Debug.LogWarning("_closeButton not found");
        if (_edgeScrollingToggle == null)
            Debug.LogWarning("_edgeScrollingToggle not found");
        if (_showControlsToggle == null)
            Debug.LogWarning("_showControlsToggle not found");
        if (_musicVolumeSlider == null)
            Debug.LogWarning("_musicVolumeSlider not found");
        if (_sfxVolumeSlider == null)
            Debug.LogWarning("_sfxVolumeSlider not found");
    }

    protected override void RegisterUICallbacksOnAwake()
    {
        _closeButton.RegisterCallback<ClickEvent>(OnCloseClicked);
        _edgeScrollingToggle.RegisterCallback<ChangeEvent<bool>>(OnEdgeScrollingToggled);
        _showControlsToggle.RegisterCallback<ChangeEvent<bool>>(OnShowControlsToggled);
        _musicVolumeSlider.RegisterCallback<ChangeEvent<float>>(OnMusicVolumeChanged);
        _sfxVolumeSlider.RegisterCallback<ChangeEvent<float>>(OnSFXVolumeChanged);
    }
    #endregion

    #region CALLBACK METHODS
    void OnCloseClicked(ClickEvent evt)
    {
        base.ExitState();
        _uiManager.BehaviourSystem.SwitchToPreviousStateInStack();
        _soundManager.PlayButtonClickSFX();
    }

    void OnEdgeScrollingToggled(ChangeEvent<bool> evt)
    {
        _uiManager.InvokeEdgeScrollingToggledEvent(evt.newValue);
        _soundManager.PlayButtonClickSFX();
    }

    void OnShowControlsToggled(ChangeEvent<bool> evt)
    {
        _uiManager.InvokeShowControlsToggledEvent(evt.newValue);
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
