using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// Controls third-person camera orbiting behavior around a target with mouse input.
/// Manages pitch (vertical) and yaw (horizontal) rotation with configurable clamping.
/// </summary>
public class ThirdPersonCameraController
{
    #region CONSTANTS
    const float FULL_ROTATION = 360f;
    #endregion

    #region PROPERTIES
    readonly Transform _cameraTarget;
    readonly float _orbitSpeed;
    readonly float _followSpeed;
    readonly float _bottomClamp;
    readonly float _topClamp;
    readonly GameManager _gameManager;

    /// <summary>Current vertical rotation angle (up/down).</summary>
    float _targetPitch;

    /// <summary>Current horizontal rotation angle (left/right).</summary>
    float _targetYaw;

    /// <summary>Mouse look input values.</summary>
    Vector2 _lookInput;
    #endregion

    #region CONSTRUCTOR
    public ThirdPersonCameraController(ThirdPersonCameraDataSO thirdPersonCameraData, CinemachineCamera camera)
    {
        _cameraTarget = camera.LookAt;
        _targetPitch = 0f;
        _targetYaw = 0f;
        _orbitSpeed = thirdPersonCameraData.OrbitSpeed;
        _followSpeed = thirdPersonCameraData.MovementSpeed;
        _bottomClamp = thirdPersonCameraData._orbitClamp[0];
        _topClamp = thirdPersonCameraData._orbitClamp[1];

        _gameManager = ServiceLocator.Instance.Get<GameManager>();
    }
    #endregion

    #region PUBLIC METHODS
    /// <summary>
    /// Syncs internal yaw/pitch with the current target rotation.
    /// </summary>
    public void SyncFromTargetRotation()
    {
        if (_cameraTarget == null)
            return;

        Vector3 eulerAngles = _cameraTarget.rotation.eulerAngles;
        _targetPitch = Mathf.DeltaAngle(0f, eulerAngles.x);
        _targetYaw = Mathf.DeltaAngle(0f, eulerAngles.y);
    }

    public void SetTargetRotation(float pitch, float yaw)
    {
        if (_cameraTarget == null)
            return;

        _targetPitch = Mathf.Clamp(pitch, _bottomClamp, _topClamp);
        _targetYaw = NormalizeAngle(yaw);

        _cameraTarget.rotation = Quaternion.Euler(_targetPitch, _targetYaw, 0f);
    }

    /// <summary>
    /// Updates camera rotation based on mouse input.
    /// Applies vertical (pitch) and horizontal (yaw) rotation to the camera target.
    /// </summary>
    public void MouseTracking()
    {
        if (_cameraTarget == null)
            return;

        _lookInput = _gameManager.InputActions.Player.Look.ReadValue<Vector2>();

        // Update pitch (vertical rotation) with clamping to prevent over-rotation
        _targetPitch = Mathf.Clamp(
            _targetPitch - _lookInput.y * _orbitSpeed * Time.deltaTime,
            _bottomClamp,
            _topClamp
        );

        // Update yaw (horizontal rotation)
        _targetYaw += _lookInput.x * _orbitSpeed * Time.deltaTime;

        // Wrap yaw to [0, 360] range
        _targetYaw = NormalizeAngle(_targetYaw);

        // Apply rotation to camera target
        _cameraTarget.rotation = Quaternion.Euler(_targetPitch, _targetYaw, 0f);
    }

    /// <summary>
    /// Smoothly moves the camera target to follow the playable character.
    /// Uses lerp interpolation for smooth position updates.
    /// </summary>
    public void TargetSmoothFollow(Transform playerTransform)
    {
        if (_cameraTarget == null)
            return;

        if (playerTransform == null)
            return;

        // Offset the camera target slightly above the player for better framing
        Vector3 newTargetPosition = new(playerTransform.position.x, playerTransform.position.y + 1f, playerTransform.position.z);

        _cameraTarget.position = Vector3.Lerp(
            _cameraTarget.position,
            newTargetPosition,
            Time.unscaledDeltaTime * _followSpeed
        );
    }
    #endregion

    #region PRIVATE METHODS
    float NormalizeAngle(float angle)
    {
        while (angle >= FULL_ROTATION)
            angle -= FULL_ROTATION;
        while (angle < 0f)
            angle += FULL_ROTATION;
        return angle;
    }
    #endregion
}