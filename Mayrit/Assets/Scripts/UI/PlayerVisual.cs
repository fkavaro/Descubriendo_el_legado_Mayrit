using System;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class PlayerVisual : Billboard
{
    UIDocument _uiDocument;
    Button _playerButton;
    PlayableCharacter _playableCharacter;

    ProgressManager _progressManager;
    UIManager _uiManager;
    CameraManager _cameraManager;

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

        // Validate dependencies
        if (_progressManager == null)
            Debug.LogError("PlayerVisual: ProgressManager not found in ServiceLocator!");
        if (_uiManager == null)
            Debug.LogError("PlayerVisual: UIManager not found in ServiceLocator!");
        if (_cameraManager == null)
            Debug.LogError("PlayerVisual: CameraManager not found in ServiceLocator!");
    }

    void Start()
    {
        // Subscribe to events and callbacks
        _progressManager.OnMilestoneChangedEvent += OnMilestoneChanged;
        _playerButton.RegisterCallback<ClickEvent>(OnPlayerButtonClick);
    }

    void LateUpdate()
    {
        CheckButtonVisibility();
    }
    #endregion

    #region PRIVATE METHODS
    void CheckButtonVisibility()
    {
        if (_playableCharacter == null)
        {
            _playerButton.visible = false;
            return;
        }

        // Hide button if not in spectator HUD state or if orbital camera is active
        if (!_uiManager.IsInSpectatorHUDState || _cameraManager.IsInOrbitalState)
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

        // Set this transform as player child
        transform.SetParent(_playableCharacter.transform);

        // Fix position
        transform.position = _playableCharacter.transform.position + 10 * Vector3.up;
    }

    void OnPlayerButtonClick(ClickEvent evt)
    {
        _cameraManager.SwitchToOrbitalCamera(_playableCharacter.GetComponent<SelectableObject>());
    }
    #endregion
}
