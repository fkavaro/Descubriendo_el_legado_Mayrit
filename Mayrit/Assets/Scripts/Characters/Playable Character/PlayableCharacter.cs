using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manages the player states and data. Singleton.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayableCharacter : ABehaviourControllable
{
    #region EDITOR PROPERTIES
    [Header("Character Information")]
    public AInformationSO _information;

    [Header("Movement settings")]
    public float _walkSpeed = 6f;
    public float _runSpeed = 12f;
    public float _rotationSpeed = 2f;
    public float _jumpForce = 2f;
    public float _gravityForce = 9f;

    [Header("Animation")]
    public Animator _animator;
    #endregion

    #region PROPERTIES
    public AAnimationController _animationController;
    public PlayerController _playerController;
    public FiniteStateMachine _fsm;
    public FreeRoam_PlayableCharacterState _freeRoamState;
    #endregion

    #region MONOBEHAVIOUR
    protected override void Awake()
    {
        // ABehaviourControllable
        base.Awake();

        _animationController = new(this, _animator);
        _playerController = new(this, GetComponent<CharacterController>());

        _fsm = new(this);
        _freeRoamState = new(_fsm, this);
        _fsm.SetInitialState(_freeRoamState);
    }

    void Start()
    {

    }

    void Update()
    {

    }
    #endregion

    #region PUBLIC METHODS
    #endregion

    #region PRIVATE METHODS
    #endregion
}
