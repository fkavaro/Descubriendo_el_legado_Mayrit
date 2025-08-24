using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manages the player states and data
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayableCharacter : MonoBehaviour, IBehaviourControllable
{
    #region PUBLIC PROPERTIES
    [Header("Behaviour Controller Properties")]
    [Tooltip("Whether to show debug messages in the console or not")]
    [SerializeField] bool _debugMode = false;
    [Tooltip("Whether to update next frame or not")]
    [SerializeField] bool _isExecutionPaused = false;

    public string Name => gameObject.name;
    public bool DebugMode
    {
        get => _debugMode;
        set => _debugMode = value;
    }
    public bool IsExecutionPaused
    {
        get => _isExecutionPaused;
        set => _isExecutionPaused = value;
    }

    [Header("Character Information")]
    public AInformationSO _information;

    [Header("Movement Controller Properties")]
    public Transform _orientation;
    public float _walkSpeed = 6f;
    public float _runSpeed = 12f;
    public float _rotationSpeed = 2f;
    public float _jumpForce = 2f;
    public float _gravityForce = 9f;

    public AAnimationController _animationController;
    [HideInInspector] public CharacterController _characterController;
    public PlayerController _playerController;

    // Finite State Machine
    public FiniteStateMachine _fsm;
    #endregion

    #region PRIVATE PROPERTIES
    #endregion

    #region INHERITED PROPERTIES
    void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _playerController = new(this);

        _animationController = new(this, GetComponentInChildren<Animator>());
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
