using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manages the player states and data
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerManager : AHumanoid<PlayerManager>
{
    #region PUBLIC PROPERTIES
    [HideInInspector] public CharacterController _characterController;
    public PlayerController _playerController;

    // Finite State Machine
    FiniteStateMachine<PlayerManager> _fsm;

    [Header("Controller Properties")]
    public Transform orientation;
    public float walkSpeed = 6f;
    public float sprintSpeed = 12f;
    public float rotationSpeed = 2f;
    public float jumpForce = 2f;
    public float gravityForce = 9f;
    #endregion

    #region PRIVATE PROPERTIES
    #endregion

    #region INHERITED PROPERTIES
    protected override void OnAwake()
    {
        _characterController = GetComponent<CharacterController>();
        _playerController = new(this);
    }

    protected override void OnStart()
    {
        _playerController.Start();
    }

    protected override void OnUpdate()
    {
        _playerController.Update();
    }

    protected override ADecisionSystem<PlayerManager> CreateDecisionSystem()
    {
        _fsm = new(this);

        return _fsm;
    }
    #endregion

    #region PUBLIC METHODS
    #endregion

    #region PRIVATE METHODS
    #endregion
}
