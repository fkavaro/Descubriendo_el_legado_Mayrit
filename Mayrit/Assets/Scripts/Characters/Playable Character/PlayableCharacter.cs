using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manages the player states and data
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayableCharacter : MonoBehaviour
{
    #region PUBLIC PROPERTIES
    public AAnimationController<PlayableCharacter> _animationController;
    [HideInInspector] public CharacterController _characterController;
    public PlayerController _playerController;

    [Header("Character Information")]
    public AInformationSO _information;

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
    void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _playerController = new(this);

        _animationController = new(name, GetComponentInChildren<Animator>());
        _fsm = new(_animationController);

        _animationController.Awake();
    }

    void Start()
    {
        _animationController.Start();
        _playerController.Start();
    }

    void Update()
    {
        _animationController.Update();
        _playerController.Update();
    }

    void LateUpdate()
    {
        _animationController.LateUpdate();
    }
    #endregion

    #region PUBLIC METHODS
    #endregion

    #region PRIVATE METHODS
    #endregion
}
