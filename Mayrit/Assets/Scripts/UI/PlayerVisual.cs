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

    void LateUpdate()
    {
        UpdateScreenPosition();
    }
    #endregion

    #region PRIVATE METHODS
    void UpdateTransformPosition()
    {
        if (_playableCharacter != null)
            transform.position = _playableCharacter.transform.position + 10 * Vector3.up;
    }

    void UpdateScreenPosition()
    {
        // Hide if no playable character is set and not in spectator HUD nor camera
        if (_playableCharacter == null &&
            !_uiManager.IsInSpectatorHUDState &&
            !_cameraManager.IsInSpectatorState)
        {
            _playerButton.visible = false;
            return;
        }

        // Get player position in world space and convert to screen space
        Vector3 worldPos = _playableCharacter.transform.position + Vector3.up;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        // Check if player is in-screen
        bool playerInScreen = screenPos.z > 0 &&
                    screenPos.x >= 0 && screenPos.x <= Screen.width &&
                    screenPos.y >= 0 && screenPos.y <= Screen.height;

        // Show button if player is in-screen
        if (playerInScreen)
            _playerButton.visible = true;
        // Hide button if not
        else
            _playerButton.visible = false;
    }
    #endregion

    #region CALLBACK METHODS
    void OnMilestoneChanged(MilestoneMapping milestoneMapping)
    {
        // Update current playable character
        _playableCharacter = milestoneMapping.PlayableCharacter;

        UpdateTransformPosition();
    }

    void OnCameraStateChanged()
    {
        if (_cameraManager.IsInSpectatorState)
            UpdateTransformPosition();
    }

    void OnPlayerButtonClick(ClickEvent evt)
    {
        _cameraManager.SwitchToOrbitalCamera(_playableCharacter.GetComponent<SelectableObject>());
        _soundManager.PlayButtonClickSFX();
    }
    #endregion
}
