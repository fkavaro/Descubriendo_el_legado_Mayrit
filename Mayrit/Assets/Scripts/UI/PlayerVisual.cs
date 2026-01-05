using System;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class PlayerVisual : Billboard
{
    UIDocument _uiDocument;
    Button _playerButton;
    PlayableCharacter _playableCharacter;

    // Dependency Injectionq
    ProgressManager _progressManager;
    UIManager _uiManager;
    CameraManager _cameraManager;
    SoundManager _soundManager;

    #region LIFE CYCLLE
    void Awake()
    {
        // Try to get the UIDocument component from the same GameObject
        _uiDocument = GetComponent<UIDocument>();
        var root = _uiDocument.rootVisualElement;

        _playerButton = root.Q<Button>(name: "PlayerButton");
        if (_playerButton == null)
        {
            Debug.LogWarning("PlayerButtonVisual: No Button with name 'PlayerButton' was found in the UIDocument.");
            return;
        }

        _playerButton.visible = false;

        // Get dependencies from Service Locator
        _progressManager = ServiceLocator.Instance.Get<ProgressManager>();
        _uiManager = ServiceLocator.Instance.Get<UIManager>();
        _cameraManager = ServiceLocator.Instance.Get<CameraManager>();
        _soundManager = ServiceLocator.Instance.Get<SoundManager>();

        // Subscribe to events and callbacks
        _progressManager.MilestoneChangedEvent += OnMilestoneChanged;
        _cameraManager.CameraStateChangedEvent += OnCameraStateChanged;
        _playerButton.RegisterCallback<ClickEvent>(OnPlayerButtonClick);
    }
    #endregion

    #region PRIVATE METHODS
    void UpdateTransformPosition()
    {
        if (_playableCharacter != null)
            transform.position = _playableCharacter.transform.position + 10 * Vector3.up;
    }
    #endregion

    #region CALLBACK METHODS
    void OnMilestoneChanged(MilestoneMapping milestoneMapping)
    {
        // Update current playable character
        _playableCharacter = milestoneMapping.PlayableCharacter;

        UpdateTransformPosition();
        OnCameraStateChanged();
    }

    void OnCameraStateChanged()
    {
        if (_cameraManager.IsInSpectatorState)
        {
            UpdateTransformPosition();
            _playerButton.visible = true;
        }
        else
            _playerButton.visible = false;
    }

    void OnPlayerButtonClick(ClickEvent evt)
    {
        _cameraManager.SwitchToOrbitalCamera(_playableCharacter.GetComponent<SelectableObject>());
        _soundManager.PlayButtonClickSFX();
    }
    #endregion
}
