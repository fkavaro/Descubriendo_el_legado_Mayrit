using UnityEngine;
using Unity.Cinemachine;

public class Orbital_CameraState : ACameraState
{
    #region PROPERTIES HELPERS
    public SelectableObject SelectedObject
    {
        get => _selectedObject;
        set => _selectedObject = value;
    }
    #endregion

    #region PROPERTIES
    readonly CinemachineOrbitalFollow _orbitalFollow;

    SelectableObject _selectedObject;
    float _orbitSpeed;
    float _zoomValue;
    float _horizontalOffset;
    #endregion

    #region CONSTRUCTOR
    public Orbital_CameraState(OrbitalCameraData orbitalCameraData)
    : base("Orbital camera", orbitalCameraData.Camera, orbitalCameraData.SimulationSpeed)
    {
        _orbitalFollow = orbitalCameraData.Camera.GetComponent<CinemachineOrbitalFollow>();
    }
    #endregion

    #region INHERITED METHODS
    public override void StartState()
    {
        base.StartState();

        if (_selectedObject == null)
        {
            Debug.LogWarning("Orbital camera state can't start without a selected object to orbit around.");
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
    #endregion

    #region PRIVATE METHODS
    void ApplyContextualPanelOffset()
    {
        _camera.GetComponent<CinemachineCameraOffset>().Offset.x = _horizontalOffset;
    }

    void AutomaticOrbit()
    {
        // Do not orbit if the game simulation is paused
        if (_gameManager.IsInPauseState) return;

        _orbitalFollow.HorizontalAxis.Value += _orbitSpeed * Time.unscaledDeltaTime;
    }
    #endregion
}