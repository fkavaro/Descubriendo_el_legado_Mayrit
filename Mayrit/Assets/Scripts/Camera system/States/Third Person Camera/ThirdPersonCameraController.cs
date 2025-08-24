using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// Controller for the third-person camera, allowing orbiting around a target.
/// </summary>
public class ThirdPersonCameraController
{
    readonly Transform _cameraTarget;
    readonly float _orbitSpeed,
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

        // Target follow current playable character and rotation is applied
        if (_cameraTarget != null)
            _cameraTarget.SetPositionAndRotation(
                GameManager.Instance.GetCurrentPlayableCharacter().transform.position,
                Quaternion.Euler(_targetPitch, _targetYaw, 0f)
            );
    }
}