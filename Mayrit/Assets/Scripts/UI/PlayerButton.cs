using UnityEngine;
using UnityEngine.UI;

public class PlayerButton : MonoBehaviour
{

    public RectTransform _playerButton;

    [Header("Offset (pixels)")]
    public Vector2 _screenOffset = new(0, 30);

    void LateUpdate()
    {
        if (_playerButton == null)
            return;

        // Hide button if game pause
        if (GameManager.Instance._fsm.IsCurrentState(GameManager.Instance._pauseState))
        {
            if (_playerButton.gameObject.activeSelf)
                _playerButton.gameObject.SetActive(false);
            return;
        }

        if (!_playerButton.gameObject.activeSelf)
            _playerButton.gameObject.SetActive(true);

        // Get player position in world space and convert to screen space
        Vector3 worldPos = GameManager.Instance._currentPlayableCharacter.transform.position + Vector3.up;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        // Check if player is in-screen
        bool playerInScreen = screenPos.z > 0 &&
                    screenPos.x >= 0 && screenPos.x <= Screen.width &&
                    screenPos.y >= 0 && screenPos.y <= Screen.height;

        // Show button if is in-screen
        _playerButton.gameObject.SetActive(playerInScreen);

        // And move button
        if (playerInScreen)
            _playerButton.position = screenPos + (Vector3)_screenOffset;
    }

    public void OnPlayerButtonClick()
    {
        if (_playerButton == null) return;

        //_image.enabled = false; // Hide button

        // Check if the player HUD state is active
        if (UIManager.Instance._playerHUDState.IsCurrentState())
            CameraManager.Instance.ToggleCameraState();
        else if (UIManager.Instance._spectatorHUDState.IsCurrentState())
            // Show the player information in contextual panel
            UIManager.Instance._spectatorHUDState.ShowContextualPanel(GameManager.Instance._currentPlayableCharacter._characterInformation);
    }
}