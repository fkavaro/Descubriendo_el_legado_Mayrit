using System;
using UnityEngine;

public class CameraManager : Singleton<CameraManager>
{
    #region PUBLIC PROPERTIES

    // Finite State Machine
    FiniteStateMachine<CameraManager> _fsm;

    [Header("Cameras")]
    public GameObject _spectatorCamera;
    public GameObject _spectatorCameraTarget;
    public GameObject _thirdPersonCamera;

    [Header("Controller Properties")]
    public float _walkSpeed = 6f;
    public float _sprintSpeed = 12f;
    public float _rotationSpeed = 2f;
    public float _jumpForce = 2f;
    public float _gravityForce = 9f;
    #endregion

    #region PRIVATE PROPERTIES
    #endregion

    #region INHERITED PROPERTIES
    protected override void OnAwake()
    {

    }

    protected override void OnStart()
    {

    }

    protected override void OnUpdate()
    {

    }

    protected override ADecisionSystem<CameraManager> CreateDecisionSystem()
    {
        _fsm = new(this);

        return _fsm;
    }

    internal void PlayPlayer()
    {
        _spectatorCamera.SetActive(false);
        _thirdPersonCamera.SetActive(true);
    }
    #endregion

    #region PUBLIC METHODS
    #endregion

    #region PRIVATE METHODS
    #endregion
}
