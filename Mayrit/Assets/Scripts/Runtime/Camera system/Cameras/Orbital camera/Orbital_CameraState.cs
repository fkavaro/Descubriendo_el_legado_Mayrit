using System;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

[Serializable]
public class OrbitalStateSetting
{
    public float OrbitSpeed => _orbitSpeed;
    public float ZoomValue => _zoomValue;
    public float HorizontalOffset => _horizontalOffset;

    public bool IsForCharacter = false;
    public DataSO DataToShow;
    public Transform Target;
    [SerializeField] float _orbitSpeed = 10f;
    [SerializeField] float _zoomValue = 70f;
    [SerializeField] float _horizontalOffset = 20f;
}

public class Orbital_CameraState : ACameraState
{
    #region PROPERTIES HELPERS
    // public SelectableObject SelectedObject
    // {
    //     get => _selectedObject;
    //     set => _selectedObject = value;
    // }

    public OrbitalStateSetting Setting
    {
        get => _setting;
        set => _setting = value;
    }
    #endregion

    #region PROPERTIES
    readonly CinemachineOrbitalFollow _orbitalFollow;

    //SelectableObject _selectedObject;
    OrbitalStateSetting _setting;
    float _orbitSpeed;
    float _zoomValue;
    float _horizontalOffset;
    #endregion

    #region CONSTRUCTOR
    public Orbital_CameraState(OrbitalCameraDataSO orbitalCameraData, CinemachineCamera camera)
    : base("Orbital camera", camera, orbitalCameraData.SimulationSpeed)
    {
        _orbitalFollow = camera.GetComponent<CinemachineOrbitalFollow>();
    }
    #endregion

    #region INHERITED METHODS
    public override void StartState()
    {
        base.StartState();

        // if (_selectedObject == null)
        // {
        //     Debug.LogWarning("Orbital camera state can't start without a selected object to orbit around.");
        //     return;
        // }

        //Transform objectToOrbitAround = _selectedObject.transform;
        _camera.Follow = _setting.Target;
        _camera.LookAt = _setting.Target;

        _orbitSpeed = _setting.OrbitSpeed;
        _zoomValue = _setting.ZoomValue;
        _horizontalOffset = _setting.HorizontalOffset;

        _orbitalFollow.Radius = _zoomValue;
        ApplyContextualPanelOffset();

        _uiManager.ShowContextualPanel(_setting.DataToShow, _setting.IsForCharacter);

        _gameManager.InputActions.Camera.Enable();
        _gameManager.InputActions.Camera.ExitMode.performed += OnExitCameraMode;
    }

    public override void UpdateState()
    {
        AutomaticOrbit();
    }

    public override void ExitState()
    {
        base.ExitState();

        _gameManager.InputActions.Camera.Disable();
        _gameManager.InputActions.Camera.ExitMode.performed -= OnExitCameraMode;
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

    private void OnExitCameraMode(InputAction.CallbackContext context)
    {
        _cameraManager.SwitchToSpectatorCamera();
        _uiManager.HideContextualPanel();
    }
}