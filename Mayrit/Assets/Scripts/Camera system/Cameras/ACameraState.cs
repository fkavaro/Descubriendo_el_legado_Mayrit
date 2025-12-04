using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;

public abstract class ACameraState : AState
{
    #region PROPERTY HELPERS
    public CinemachineCamera Camera
    {
        get => _camera;
        set => _camera = value;
    }
    #endregion

    #region PROPERTIES
    protected CinemachineCamera _camera;
    protected readonly float _simulationSpeed;

    // Dependency Injection
    protected TimeManager _timeManager;
    protected UIManager _uiManager;
    protected GameManager _gameManager;
    protected CameraManager _cameraManager;
    #endregion

    #region CONSTRUCTOR
    protected ACameraState(string name,
        CinemachineCamera camera,
        float simulationSpeed)
    : base(name)
    {
        _camera = camera;
        _simulationSpeed = simulationSpeed;
    }
    #endregion

    #region INHERITED METHODS
    public override void StartState()
    {
        if (_camera == null)
        {
            Debug.LogWarning($"ACameraState.StartState: Cannot start camera state {StateName} because the CinemachineCamera reference is null.");
            return;
        }

        // Get dependencies from ServiceLocator
        _timeManager = ServiceLocator.Instance.Get<TimeManager>();
        _uiManager = ServiceLocator.Instance.Get<UIManager>();
        _gameManager = ServiceLocator.Instance.Get<GameManager>();
        _cameraManager = ServiceLocator.Instance.Get<CameraManager>();

        // Validate dependencies
        if (_timeManager == null)
        {
            Debug.LogError($"ACameraState.StartState: TimeManager not found when starting camera state {StateName}.");
            return;
        }
        else
            _timeManager.SetSimulationSpeed(_simulationSpeed);

        if (_uiManager == null)
        {
            Debug.LogError($"ACameraState.StartState: UIManager not found when starting camera state {StateName}.");
            return;
        }
        if (_gameManager == null)
        {
            Debug.LogError($"ACameraState.StartState: GameManager not found when starting camera state {StateName}.");
            return;
        }
        if (_cameraManager == null)
        {
            Debug.LogError($"ACameraState.StartState: CameraManager not found when starting camera state {StateName}.");
            return;
        }

        OnStateStarted();

        _camera.gameObject.SetActive(true);
    }

    public override void ExitState()
    {
        OnStateExited();

        _camera.gameObject.SetActive(false);
    }
    #endregion

    #region VIRTUAL METHODS
    public abstract void OnStateStarted();
    public abstract void OnStateExited();
    #endregion
}