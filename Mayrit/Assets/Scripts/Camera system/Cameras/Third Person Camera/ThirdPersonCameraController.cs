using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// Controller for the third-person camera, allowing orbiting around a target.
/// </summary>
public class ThirdPersonCameraController
{
    #region PROPERTIES
    readonly Transform _cameraTarget;
    readonly float _orbitSpeed,
        _followSpeed,
        _bottomClamp,
        _topClamp;

    float _targetPitch, // Horizontal rotation
        _targetYaw; // Vertical rotation

    // Input
    Vector2 _lookInput;

    // Dependency Injection
    readonly CameraManager _cameraManager;
    readonly GameManager _gameManager;
    #endregion

    #region CONSTRUCTOR
    public ThirdPersonCameraController(CinemachineCamera camera)
    {
        _cameraTarget = camera.LookAt;
        _targetPitch = 0f;
        _targetYaw = 0f;

        // Get dependencies from ServiceLocator
        _cameraManager = ServiceLocator.Instance.Get<CameraManager>();
        _gameManager = ServiceLocator.Instance.Get<GameManager>();

        // Validate dependencies
        if (_cameraManager == null)
            Debug.LogError("ThirdPersonCameraController: CameraManager not found in ServiceLocator!");
        else
        {
            _orbitSpeed = _cameraManager._3rdPersonCameraOrbitSpeed;
            _followSpeed = _cameraManager._3rdPersonCameraFollowSpeed;
            _bottomClamp = _cameraManager._bottomClamp;
            _topClamp = _cameraManager._topClamp;
        }

        if (_gameManager == null)
            Debug.LogError("ThirdPersonCameraController: GameManager not found in ServiceLocator!");
    }
    #endregion

    #region PUBLIC METHODS
    public void MouseTracking()
    {
        // Read input
        _lookInput = _gameManager.InputActions.Player.Look.ReadValue<Vector2>();

        // Update pitch and yaw based on input
        _targetPitch = Mathf.Clamp(_targetPitch - _lookInput.y * _orbitSpeed * Time.deltaTime, _bottomClamp, _topClamp);
        _targetYaw += _lookInput.x * _orbitSpeed * Time.deltaTime;

        // Clamp yaw
        if (_targetYaw > 360f) _targetYaw -= 360f;
        if (_targetYaw < 0f) _targetYaw += 360f;

        if (_cameraTarget == null) return;

        // Apply rotation to camera target
        _cameraTarget.rotation = Quaternion.Euler(_targetPitch, _targetYaw, 0f);
    }

    public void TargetSmoothFolow()
    {
        if (_cameraTarget == null) return;

        // Move smoothly camera target to follow the player
        Transform player = _gameManager.PlayableCharacter.transform;
        _cameraTarget.position = Vector3.Lerp(_cameraTarget.position, player.position, Time.unscaledDeltaTime * _followSpeed);
    }
    #endregion
}