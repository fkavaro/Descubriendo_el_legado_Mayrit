using UnityEngine;
using Unity.Cinemachine;

public class Orbital_CameraState : ACameraState
{
    public readonly CinemachineOrbitalFollow _orbitalFollow;

    public AInformationSO _information;
    float _orbitSpeed,
        _zoomValue,
        _horizontalOffset;


    Vector2 _lookInput;
    bool _middleClickPressed;
    float _orbitSmoothing;

    public Orbital_CameraState(FiniteStateMachine<CameraManager> stateMachine,
        CinemachineCamera camera)
        : base("Orbital camera", stateMachine, camera)
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
    }

    public override void UpdateState()
    {
        //AutomaticOrbit();
        //InputOrbit();
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

    void InputOrbit()
    {
        _lookInput = GameManager.Instance._inputActions.Camera.Look.ReadValue<Vector2>();
        _middleClickPressed = GameManager.Instance._inputActions.Camera.Rotate.IsPressed();
        _orbitSmoothing = CameraManager.Instance._orbitSmoothing;

        Vector2 orbitInput = _lookInput * (_middleClickPressed ? 1f : 0f);

        orbitInput *= _orbitSpeed;

        InputAxis horizontalRotation = _orbitalFollow.HorizontalAxis;
        InputAxis verticalRotation = _orbitalFollow.VerticalAxis;

        horizontalRotation.Value = Mathf.Lerp(horizontalRotation.Value, horizontalRotation.Value + orbitInput.x, _orbitSmoothing * Time.unscaledDeltaTime);
        verticalRotation.Value = Mathf.Lerp(verticalRotation.Value, verticalRotation.Value - orbitInput.y, _orbitSmoothing * Time.unscaledDeltaTime);

        verticalRotation.Value = Mathf.Clamp(verticalRotation.Value, verticalRotation.Range.x, verticalRotation.Range.y);

        _orbitalFollow.HorizontalAxis = horizontalRotation;
        _orbitalFollow.VerticalAxis = verticalRotation;
    }
}