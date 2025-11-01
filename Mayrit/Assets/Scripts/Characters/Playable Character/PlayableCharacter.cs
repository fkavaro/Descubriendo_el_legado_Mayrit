using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayableCharacter : ABehaviourEntity<FiniteStateMachine>
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

    #region INTERNAL PROPERTIES
    public AnimationController _animationController;
    public PlayerController _playerController;

    FiniteStateMachine _fsm;
    public FreeRoam_PlayableCharacterState _freeRoamState;
    #endregion

    #region INHERITED
    public override FiniteStateMachine InitializeBehaviourSystem()
    {
        _fsm = new(this);

        _freeRoamState = new(_fsm, this);

        _fsm.SetInitialState(_freeRoamState);

        return _fsm;
    }
    #endregion

    #region MONOBEHAVIOUR
    protected override void Awake()
    {
        base.Awake();

        _animationController = new(this, this, _animator);
        _playerController = new(this, GetComponent<CharacterController>());
    }
    #endregion
}
