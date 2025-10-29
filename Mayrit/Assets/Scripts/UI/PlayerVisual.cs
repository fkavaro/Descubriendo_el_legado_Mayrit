using System;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class PlayerVisual : Billboard
{
    UIDocument _uiDocument;
    Button _playerButton;
    public PlayableCharacter _playableCharacter;

    void Awake()
    {
        // Try to get the UIDocument component from the same GameObject
        _uiDocument = GetComponent<UIDocument>();
        var root = _uiDocument.rootVisualElement;

        // Try to find a Label with name 'Name' in the document
        _playerButton = root.Q<Button>(name: "PlayerButton");
        if (_playerButton == null)
        {
            Debug.LogWarning("PlayerButtonVisual: No Button with name 'PlayerButton' was found in the UIDocument.");
            return;
        }

        _playerButton.visible = false;

        // Subscribe to milestone change event
        ProgressManager.Instance.OnMilestoneChanged += UpdatePlayerButtonVisual;

        // Register click event
        _playerButton.RegisterCallback<ClickEvent>(OnPlayerButtonClick);
    }

    void Start()
    {
        // Update current playable character
        _playableCharacter = GameManager.Instance._currentPlayableCharacter;
    }

    void LateUpdate()
    {
        CheckButtonVisibility();
    }

    void CheckButtonVisibility()
    {
        // Hide button if not in spectator HUD state or if orbital camera is active
        if (!UIManager.Instance._spectatorHUDState.IsCurrentState() ||
            CameraManager.Instance._orbitalState.IsCurrentState())
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

    void UpdatePlayerButtonVisual(ProgressManager.Milestone milestone)
    {
        // Update current playable character
        _playableCharacter = GameManager.Instance._currentPlayableCharacter;

        // Set this transform as player child
        transform.SetParent(_playableCharacter.transform);

        // Fix position
        transform.position = _playableCharacter.transform.position + 10 * Vector3.up;
    }

    void OnPlayerButtonClick(ClickEvent evt)
    {
        // Spectator camera
        if (CameraManager.Instance._spectatorState.IsCurrentState())
            CameraManager.Instance.SwitchToOrbitalCamera(_playableCharacter.transform, _playableCharacter._information);
        // Third person camera
        else if (CameraManager.Instance._thirdPersonState.IsCurrentState())
            CameraManager.Instance.SwitchToSpectatorCamera();
    }
}
