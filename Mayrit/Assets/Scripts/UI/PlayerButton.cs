using UnityEngine;
using UnityEngine.UI;

public class PlayerButton : MonoBehaviour
{

    public RectTransform _playerButton;

    [Header("Offset (pixels)")]
    public Vector2 _screenOffset = new(0, 30);

    PlayableCharacter _playableCharacter;

    void LateUpdate()
    {
        if (_playerButton == null)
            return;

        // Rerturn if game UI isn't spectator hud
        if (!UIManager.Instance._fsm.IsCurrentState(UIManager.Instance._spectatorHUDState))
            return;

        // Current playable character has changed
        if (_playableCharacter != GameManager.Instance._currentPlayableCharacter)
            _playableCharacter = GameManager.Instance._currentPlayableCharacter;

        // Get player position in world space and convert to screen space
        Vector3 worldPos = _playableCharacter.transform.position + Vector3.up;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        // Check if player is in-screen
        bool playerInScreen = screenPos.z > 0 &&
                    screenPos.x >= 0 && screenPos.x <= Screen.width &&
                    screenPos.y >= 0 && screenPos.y <= Screen.height;

        // Show button if game UI is spectator hud and player is in-screen
        if (UIManager.Instance._fsm.IsCurrentState(UIManager.Instance._spectatorHUDState)
            && playerInScreen)
        {
            if (!_playerButton.gameObject.activeSelf)
                _playerButton.gameObject.SetActive(true);

            // Update button position
            if (playerInScreen)
                _playerButton.position = screenPos + (Vector3)_screenOffset;
        }
        // Hide button
        else
            _playerButton.gameObject.SetActive(false);
    }

    public void OnPlayerButtonClick()
    {
        if (_playerButton == null) return;

        // Spectator camera
        if (CameraManager.Instance._spectatorState.IsCurrentState())
            CameraManager.Instance.SwitchToOrbitalCamera(_playableCharacter.transform, _playableCharacter._information);
        // Third person camera
        else if (CameraManager.Instance._thirdPersonState.IsCurrentState())
            CameraManager.Instance.SwitchToSpectatorCamera();
    }
}