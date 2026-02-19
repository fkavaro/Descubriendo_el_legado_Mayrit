using UnityEngine;
using Unity.Cinemachine;

public class OrbitalCameraController
{
    #region PROPERTIES
    readonly CinemachineOrbitalFollow _orbitalFollow;
    readonly CinemachineCamera _camera;
    readonly float _orbitSmoothing;
    readonly GameManager _gameManager;

    Vector2 _lookInput;
    float _orbitSpeed;
    float _zoomValue;
    float _horizontalOffset;
    bool _middleClickPressed;
    #endregion

    #region CONSTRUCTOR
    public OrbitalCameraController(OrbitalCameraDataSO orbitalCameraData, CinemachineCamera camera)
    {
        _camera = camera;
        _orbitalFollow = camera.GetComponent<CinemachineOrbitalFollow>();
        _orbitSmoothing = orbitalCameraData.orbitSmoothing;
        _gameManager = ServiceLocator.Instance.Get<GameManager>();
    }
    #endregion

    #region PUBLIC METHODS
    public void Start(OrbitalStateSetting _setting)
    {
        _camera.Follow = _setting.Target;
        _camera.LookAt = _setting.Target;

        _orbitSpeed = _setting.OrbitSpeed;
        _zoomValue = _setting.ZoomValue;
        _horizontalOffset = _setting.HorizontalOffset;

        _orbitalFollow.Radius = _zoomValue;
        ApplyContextualPanelOffset();
    }

    public void LateUpdate()
    {
        _middleClickPressed = _gameManager.InputActions.Camera.Rotate.IsPressed();
        _lookInput = _gameManager.InputActions.Camera.Look.ReadValue<Vector2>();

        if (_middleClickPressed)
            ManualOrbiting();
        else
            AutomaticOrbiting();
    }

    public void ApplyContextualPanelOffset()
    {
        _camera.GetComponent<CinemachineCameraOffset>().Offset.x = _horizontalOffset;
    }
    #endregion

    #region PRIVATE METHODS
    void AutomaticOrbiting()
    {
        _orbitalFollow.HorizontalAxis.Value += _orbitSpeed * Time.unscaledDeltaTime;
    }

    void ManualOrbiting()
    {
        Vector2 rotationDelta = _lookInput * _orbitSpeed;

        // Get current rotation axes
        InputAxis horizontalAxis = _orbitalFollow.HorizontalAxis;
        InputAxis verticalAxis = _orbitalFollow.VerticalAxis;

        // Apply rotation with smoothing
        float smoothFactor = _orbitSmoothing * Time.unscaledDeltaTime;
        horizontalAxis.Value = Mathf.Lerp(horizontalAxis.Value, horizontalAxis.Value + rotationDelta.x, smoothFactor);
        verticalAxis.Value = Mathf.Lerp(verticalAxis.Value, verticalAxis.Value - rotationDelta.y, smoothFactor);

        // Clamp vertical rotation to prevent flipping
        verticalAxis.Value = Mathf.Clamp(verticalAxis.Value, verticalAxis.Range.x, verticalAxis.Range.y);

        // Write back modified axes
        _orbitalFollow.HorizontalAxis = horizontalAxis;
        _orbitalFollow.VerticalAxis = verticalAxis;
    }
    #endregion
}
