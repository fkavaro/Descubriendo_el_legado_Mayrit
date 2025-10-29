using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// Controller for the third-person camera, allowing orbiting around a target.
/// </summary>
public class ThirdPersonCameraController
{
    readonly Transform _cameraTarget;
    readonly float _orbitSpeed,
        _followSpeed,
        _bottomClamp,
        _topClamp;

    float _targetPitch, // Horizontal rotation
        _targetYaw; // Vertical rotation

    // Input
    Vector2 _lookInput;

    public ThirdPersonCameraController(CinemachineCamera camera)
    {
        _cameraTarget = camera.LookAt;
        _targetPitch = 0f;
        _targetYaw = 0f;
        _orbitSpeed = CameraManager.Instance._3rdPersonCameraOrbitSpeed;
        _followSpeed = CameraManager.Instance._3rdPersonCameraFollowSpeed;
        _bottomClamp = CameraManager.Instance._bottomClamp;
        _topClamp = CameraManager.Instance._topClamp;
    }

    public void LateUpdate()
    {
        // Read input
        _lookInput = GameManager.Instance._inputActions.Player.Look.ReadValue<Vector2>();

        // Update pitch and yaw based on input
        _targetPitch = Mathf.Clamp(_targetPitch - _lookInput.y * _orbitSpeed * Time.deltaTime, _bottomClamp, _topClamp);
        _targetYaw += _lookInput.x * _orbitSpeed * Time.deltaTime;

        // Clamp yaw
        if (_targetYaw > 360f) _targetYaw -= 360f;
        if (_targetYaw < 0f) _targetYaw += 360f;

        if (_cameraTarget != null)
        {
            // Apply rotation to camera target
            _cameraTarget.rotation = Quaternion.Euler(_targetPitch, _targetYaw, 0f);

            // Move smoothly camera target to follow the player
            PlayableCharacter player = GameManager.Instance._currentPlayableCharacter;
            _cameraTarget.position = Vector3.Lerp(_cameraTarget.position, player.transform.position, Time.unscaledDeltaTime * _followSpeed);
        }
    }
}