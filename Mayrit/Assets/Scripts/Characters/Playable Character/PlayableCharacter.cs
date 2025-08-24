using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manages the player states and data. Singleton.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayableCharacter : MonoBehaviour, IBehaviourControllable
{
    #region EDITOR PROPERTIES
    [Header("Behaviour Controller Properties")]
    [Tooltip("Whether to show debug messages in the console or not")]
    [SerializeField] bool _debugMode = false;
    [Tooltip("Whether to update next frame or not")]
    [SerializeField] bool _isExecutionPaused = false;

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

    #region PROPERTIES
    [HideInInspector] public CharacterController _characterController;
    public AAnimationController _animationController;
    public PlayerController _playerController;

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

    public FiniteStateMachine _fsm;
    #endregion

    #region MONOBEHAVIOUR
    void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _playerController = new(this);

        _fsm = new(this);

        _animationController = new(_fsm, GetComponentInChildren<Animator>());
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
