using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class OrbitalCameraController
{
    readonly CinemachineOrbitalFollow _orbitalFollow;
    readonly OrbitalCameraDataSO _orbitalCameraData;
    readonly CinemachineCamera _camera;

    float _orbitSpeed;
    float _zoomValue;
    float _horizontalOffset;

    public OrbitalCameraController(OrbitalCameraDataSO orbitalCameraData, CinemachineCamera camera)
    {
        _orbitalCameraData = orbitalCameraData;
        _camera = camera;
        _orbitalFollow = camera.GetComponent<CinemachineOrbitalFollow>();
    }

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

    public void ApplyContextualPanelOffset()
    {
        _camera.GetComponent<CinemachineCameraOffset>().Offset.x = _horizontalOffset;
    }

    public void Update()
    {
        _orbitalFollow.HorizontalAxis.Value += _orbitSpeed * Time.unscaledDeltaTime;
    }
}
