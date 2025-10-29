using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manages the player states and data. Singleton.
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(FiniteStateMachine))]
public class PlayableCharacter : ABehaviourControllable
{
    #region EDITOR PROPERTIES
    [Header("Character Information")]
    public AInformationSO _information;

    [Header("Movement settings")]
    public float _walkSpeed = 1f;
    public float _sprintSpeed = 3f;
    public float _rotationSpeed = 4f;
    public float _jumpForce = 2f;
    public float _gravityForce = 9f;

    [Header("Animation")]
    public Animator _animator;
    #endregion

    #region PROPERTIES
    public AnimationController _animationController;
    public PlayerController _playerController;
    [HideInInspector] public FiniteStateMachine _fsm;
    public FreeRoam_PlayableCharacterState _freeRoamState;
    #endregion

    #region INHERITED
    void Awake()
    {
        _animationController = new(_fsm, _animator);
        _playerController = new(this, GetComponent<CharacterController>());
    }

    public override void SetDecisionSystem()
    {
        // FINITE STATE MACHINE
        _fsm = GetComponent<FiniteStateMachine>();
        _freeRoamState = new(_fsm, this);
        _fsm.SetInitialState(_freeRoamState);

        _fsm.enabled = true; // Ensure FSM is enabled
    }
    #endregion
}
