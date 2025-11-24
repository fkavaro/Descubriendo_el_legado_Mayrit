using UnityEngine;
using Unity.Cinemachine;

public class Orbital_CameraState : ACameraState
{
    public readonly CinemachineOrbitalFollow _orbitalFollow;

    public AInformationSO _information;
    float _orbitSpeed,
        _zoomValue,
        _horizontalOffset;

    public Orbital_CameraState(CinemachineCamera camera, float simulationSpeed)
    : base("Orbital camera", camera, simulationSpeed)
    {
        _orbitalFollow = camera.GetComponent<CinemachineOrbitalFollow>();
    }

    public override void StartState()
    {
        GameManager.Instance._inputActions.Camera.Enable();

        // Is character information
        if (_information is Character_InformationSO)
        {
            _orbitSpeed = CameraManager.Instance._orbitalCharacterOrbitSpeed;
            _zoomValue = CameraManager.Instance._orbitalCharacterZoom;
            _horizontalOffset = CameraManager.Instance._orbitalCharacterOffset;
        }
        // Building or other
        else
        {
            _orbitSpeed = CameraManager.Instance._orbitalBuildingOrbitSpeed;
            _zoomValue = CameraManager.Instance._orbitalBuildingZoom;
            _horizontalOffset = CameraManager.Instance._orbitalBuildingOffset;
        }

        _orbitalFollow.Radius = _zoomValue;
        ApplyContextualPanelOffset();

        _camera.gameObject.SetActive(true);
        UIManager.Instance._spectatorHUDState.ShowContextualPanel(_information);

        // Adjust simulation speed
        TimeManager.Instance.SetSimulationSpeed(_simulationSpeed);
    }

    public override void UpdateState()
    {
        AutomaticOrbit();
    }

    public override void ExitState()
    {
        _camera.gameObject.SetActive(false);
        GameManager.Instance._inputActions.Camera.Disable();
    }

    void ApplyContextualPanelOffset()
    {
        _camera.GetComponent<CinemachineCameraOffset>().Offset.x = _horizontalOffset;
    }

    void AutomaticOrbit()
    {
        _orbitalFollow.HorizontalAxis.Value += _orbitSpeed * Time.unscaledDeltaTime;
    }
}