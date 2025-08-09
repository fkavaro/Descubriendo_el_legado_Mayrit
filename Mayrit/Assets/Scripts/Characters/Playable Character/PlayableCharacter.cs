using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manages the player states and data
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayableCharacter : AAnimationController<PlayableCharacter>
{
    #region PUBLIC PROPERTIES
    [HideInInspector] public CharacterController _characterController;
    public PlayerController _playerController;

    [Header("Character Information")]
    public CharacterInformationSO _characterInformation;

    [Header("Movement Controller Properties")]
    public Transform _orientation;
    public float _walkSpeed = 6f;
    public float _runSpeed = 12f;
    public float _rotationSpeed = 2f;
    public float _jumpForce = 2f;
    public float _gravityForce = 9f;
    #endregion

    #region PRIVATE PROPERTIES
    // Finite State Machine
    FiniteStateMachine<PlayableCharacter> _fsm;
    #endregion

    #region INHERITED PROPERTIES
    protected override void OnAwake()
    {
        base.OnAwake();

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

    protected override ADecisionSystem<PlayableCharacter> CreateDecisionSystem()
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
