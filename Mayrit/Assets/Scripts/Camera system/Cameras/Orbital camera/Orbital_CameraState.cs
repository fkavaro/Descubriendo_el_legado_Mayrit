using UnityEngine;
using Unity.Cinemachine;

public class Orbital_CameraState : ACameraState
{
    public SelectableObject SelectedObject
    {
        get => _selectedObject;
        set => _selectedObject = value;
    }

    readonly CinemachineOrbitalFollow _orbitalFollow;

    SelectableObject _selectedObject;
    float _orbitSpeed;
    float _zoomValue;
    float _horizontalOffset;


    public Orbital_CameraState(CinemachineCamera camera, float simulationSpeed)
    : base("Orbital camera", camera, simulationSpeed)
    {
        _orbitalFollow = camera.GetComponent<CinemachineOrbitalFollow>();
    }

    public override void OnStateStarted()
    {
        if (_selectedObject == null)
        {
            Debug.LogWarning("Orbital camera state started without a selected object to orbit around.");
            return;
        }

        Transform objectToOrbitAround = _selectedObject.transform;
        _camera.Follow = objectToOrbitAround;
        _camera.LookAt = objectToOrbitAround;

        _orbitSpeed = _selectedObject.OrbitalCameraValues.OrbitSpeed;
        _zoomValue = _selectedObject.OrbitalCameraValues.ZoomValue;
        _horizontalOffset = _selectedObject.OrbitalCameraValues.HorizontalOffset;

        _orbitalFollow.Radius = _zoomValue;
        ApplyContextualPanelOffset();

        _uiManager.ShowContextualPanel(_selectedObject.Data, _selectedObject.IsCharacter);
    }

    public override void UpdateState()
    {
        AutomaticOrbit();
    }

    public override void OnStateExited() { }

    void ApplyContextualPanelOffset()
    {
        _camera.GetComponent<CinemachineCameraOffset>().Offset.x = _horizontalOffset;
    }

    void AutomaticOrbit()
    {
        // Do not orbit if the game simulation is paused
        if (Time.timeScale == 0f) return;

        _orbitalFollow.HorizontalAxis.Value += _orbitSpeed * Time.unscaledDeltaTime;
    }
}