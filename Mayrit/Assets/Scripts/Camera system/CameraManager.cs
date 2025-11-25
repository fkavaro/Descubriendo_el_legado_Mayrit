using System;
using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// Manages the camera states and data. Singleton.
/// </summary>
public class CameraManager : ASingletonBehaviourEntity<CameraManager, FiniteStateMachine<ACameraState>>
{
    #region PROPERTY HELPERS
    public bool IsInSpectatorState => _fsm.IsCurrentState(_spectatorState);
    public bool IsInOrbitalState => _fsm.IsCurrentState(_orbitalState);
    public bool IsInThirdPersonState => _fsm.IsCurrentState(_thirdPersonState);
    #endregion

    #region EDITOR PROPERTIES
    [Header("Spectator camera")]
    [Range(0.1f, 5f)]
    public float _spectatorSimSpeed = 3f;
    public CinemachineCamera _spectatorCamera;
    [Tooltip("Wether to move camera at screen margins or not.")]
    public bool _edgeScrolling = false;
    public int _edgeScrollingMargin = 30;
    public float _moveSpeed = 500f;
    public AnimationCurve _moveSpeedZoomCurve = AnimationCurve.Linear(0f, 0.1f, 1f, 1f);
    public float _acceleration = 200f;
    public float _deceleration = 250f;
    public float _sprintSpeedMultiplier = 2f;
    [Tooltip("Camera limits in X axis (min, max)")]
    public Vector2 _movementLimitsX = new(-1000, 700);
    [Tooltip("Camera limits in Y axis (min: at which target will be positioned, max: max distance from target)")]
    public Vector2 _movementLimitsY = new(120, 400);
    [Tooltip("Camera limits in Z axis (min, max)")]
    public Vector2 _movementLimitsZ = new(-800, 800);
    [Tooltip("Mouse sensitivity for camera rotation.")]
    public float _spectatorCameraOrbitSpeed = 0.5f;
    public float _orbitSmoothing = 5f;
    [Tooltip("Speed of camera zoom with scroll wheel.")]
    public float _zoomSpeed = 0.1f;
    public float _zoomSmoothing = 5f;
    [Tooltip("Layer mask to define which objects are selectable.")]
    public LayerMask _selectableLayer;
    [Tooltip("Speed to move the spectator camera target when switching from third person camera.")]
    public float _spectatorTargetFixingSpeed = 40f;

    [Header("Orbital camera")]
    [Range(0.1f, 5f)]
    public float _orbitalSimSpeed = 1f;
    public CinemachineCamera _orbitalCamera;
    public float _orbitalBuildingOrbitSpeed = 30f;
    public float _orbitalBuildingZoom;
    public float _orbitalBuildingOffset = 20f;
    public float _orbitalCharacterOrbitSpeed = 15f;
    public float _orbitalCharacterZoom;
    public float _orbitalCharacterOffset = 10f;

    [Header("Third Person Camera")]
    [Range(0.1f, 5f)]
    public float _thirdPersonSimSpeed = 1f;
    public CinemachineCamera _thirdPersonCamera;
    public float _3rdPersonCameraOrbitSpeed = 3f,
        _3rdPersonCameraFollowSpeed = 3f,
        _bottomClamp = -30f,
        _topClamp = 40f;
    #endregion

    #region INTERNAL PROPERTIES
    public event Action OnCameraStateChangedEvent;

    FiniteStateMachine<ACameraState> _fsm;
    Spectator_CameraState _spectatorState;
    ThirdPerson_CameraState _thirdPersonState;
    Orbital_CameraState _orbitalState;
    #endregion

    #region INHERITED
    public override FiniteStateMachine<ACameraState> InitializeBehaviourSystem()
    {
        _fsm = new(this);

        // States initialization
        _spectatorState = new(_spectatorCamera, _spectatorSimSpeed);
        _orbitalState = new(_orbitalCamera, _orbitalSimSpeed);
        _thirdPersonState = new(_thirdPersonCamera, _thirdPersonSimSpeed);

        _fsm.SetInitialState(_spectatorState);

        return _fsm;
    }
    #endregion

    #region MONOBEHAVIOUR
    protected override void Start()
    {
        base.Start();

        // Set camera target at min height
        CinemachineOrbitalFollow _orbitalFollow = _spectatorCamera.GetComponent<CinemachineOrbitalFollow>();
        _orbitalFollow.Radius = _movementLimitsY.y;

        if (_spectatorCamera.LookAt.position.y != _movementLimitsY.x)
        {
            // Fix spectator target height
            _spectatorCamera.LookAt.position = new(
                _spectatorCamera.LookAt.position.x,
                _movementLimitsY.x,
                _spectatorCamera.LookAt.position.z);
        }
    }
    #endregion

    #region PUBLIC METHODS
    public void SwitchToSpectatorCamera()
    {
        if (_fsm.IsCurrentState(_thirdPersonState))
        {
            _spectatorCamera.LookAt.position = _thirdPersonCamera.LookAt.position;

            // Fix to look at target height
            Vector3 fixedSpectatorLookAt = new(
                _spectatorCamera.LookAt.position.x,
                _movementLimitsY.x, // At spectator height
                _spectatorCamera.LookAt.position.z
            );

            _fsm.SwitchState(_spectatorState);

            SmoothMoveCoroutine(_spectatorCamera.LookAt, fixedSpectatorLookAt, _spectatorTargetFixingSpeed);
        }
        else if (_fsm.IsCurrentState(_orbitalState))
        {
            _spectatorCamera.LookAt.position = _orbitalCamera.LookAt.position;

            // Same orbit values as orbital camera
            _spectatorCamera.GetComponent<CinemachineOrbitalFollow>().HorizontalAxis.Value =
                _orbitalCamera.GetComponent<CinemachineOrbitalFollow>().HorizontalAxis.Value;
            _spectatorCamera.GetComponent<CinemachineOrbitalFollow>().VerticalAxis.Value =
                _orbitalCamera.GetComponent<CinemachineOrbitalFollow>().VerticalAxis.Value;

            // Fix to look at target height
            Vector3 fixedSpectatorLookAt = new(
                _spectatorCamera.LookAt.position.x,
                _movementLimitsY.x, // At spectator height
                _spectatorCamera.LookAt.position.z
            );

            _spectatorCamera.LookAt.position = fixedSpectatorLookAt;

            _fsm.SwitchState(_spectatorState);
        }

        OnCameraStateChangedEvent?.Invoke();
    }

    public void SwitchToOrbitalCamera(Transform objectToOrbitAround, AInformationSO information)
    {
        // Hide contextual panel
        UIManager.Instance._spectatorHUDState._contextualPanel.Hide();

        _orbitalState._information = information;
        _orbitalCamera.Follow = objectToOrbitAround;
        _orbitalCamera.LookAt = objectToOrbitAround;

        // Same orbit values as spectator camera
        _orbitalCamera.GetComponent<CinemachineOrbitalFollow>().HorizontalAxis.Value =
            _spectatorCamera.GetComponent<CinemachineOrbitalFollow>().HorizontalAxis.Value;
        _orbitalCamera.GetComponent<CinemachineOrbitalFollow>().VerticalAxis.Value =
            _spectatorCamera.GetComponent<CinemachineOrbitalFollow>().VerticalAxis.Value;

        _fsm.SwitchState(_orbitalState);

        OnCameraStateChangedEvent?.Invoke();
    }

    public void SwitchToThirdPersonCamera()
    {
        // Update third person camera target to current playable character
        Transform playerTranform = GameManager.Instance._playableCharacter.transform;

        // Set camera follow and look at targets
        _thirdPersonCamera.LookAt.position = playerTranform.position;

        _fsm.SwitchState(_thirdPersonState);

        OnCameraStateChangedEvent?.Invoke();
    }

    /// <summary>
    /// Moves smoothly the given transform to the new position in given duration.
    /// </summary>
    public void SmoothMoveCoroutine(Transform lookAt, Vector3 newPosition, float speed, Action onComplete = null)
    {
        StartCoroutine(SmoothMove(lookAt, newPosition, speed, onComplete));
    }
    #endregion

    #region PRIVATE METHODS
    /// <summary>
    /// Smoothly moves the given transform to the new position in given speed.
    /// </summary>
    IEnumerator SmoothMove(Transform transform, Vector3 newPosition, float speed, Action onComplete)
    {
        if (transform == null)
            yield break;

        Vector3 startPosition = transform.position;
        float distance = Vector3.Distance(startPosition, newPosition);
        if (distance < 0.001f)
        {
            transform.position = newPosition;
            onComplete?.Invoke();
            yield break;
        }

        float moved = 0f;
        while (moved < distance)
        {
            float step = speed * Time.unscaledDeltaTime;
            moved += step;
            float t = Mathf.Clamp01(moved / distance);
            transform.position = Vector3.Lerp(startPosition, newPosition, t);
            yield return null;
        }

        transform.position = newPosition;
        onComplete?.Invoke();
    }
    #endregion
}
