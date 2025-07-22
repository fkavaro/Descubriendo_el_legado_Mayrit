using UnityEngine;

public class PlayerController
{
    #region PRIVATE PROPERTIES
    readonly PlayerManager _playerManager;

    float _verticalVelocity,
        _movementSpeed;

    Vector3 _movement3D,
        _forward,
        _right;

    Vector2 _movement2D;

    Transform _cameraTransform,
        _orientation;
    #endregion

    #region PUBLIC METHODS
    public PlayerController(PlayerManager playerManager)
    {
        _playerManager = playerManager;
    }

    public void Start()
    {
        _cameraTransform = Camera.main.transform;
        _orientation = _playerManager.orientation;
    }

    public void Update()
    {
        // Read input actions from GameManager
        _movement2D = GameManager.Instance._inputActions.Player.Move.ReadValue<Vector2>();

        HandleGravity();
        HandleMovement();
        HandleRotation();
    }
    #endregion

    #region PRIVATE METHODS
    /// <summary>
    /// Calculates vertical velocity
    /// </summary>
    void HandleGravity()
    {
        // Player on ground
        if (_playerManager._characterController.isGrounded)
        {
            // Jump key pressed
            if (GameManager.Instance._inputActions.Player.Jump.IsPressed())
                _verticalVelocity = _playerManager.jumpForce; // Jump
            // Not pressed
            else
                _verticalVelocity = -1f; // Small gravity to keep grounded
        } // Not on ground
        else
        {
            // Apply gravity to vertical velocity
            _verticalVelocity -= _playerManager.gravityForce * Time.deltaTime;
        }
    }

    /// <summary>
    /// Transforms 2D input into 3D movement, applying vertical velocity
    /// </summary>
    void HandleMovement()
    {
        // Sprint key pressed
        if (GameManager.Instance._inputActions.Player.Sprint.IsPressed())
            _movementSpeed = _playerManager.sprintSpeed; // Move with sprint speed
        else
            _movementSpeed = _playerManager.walkSpeed; // Move with walk speed

        // Get direction in 3D space based on camera orientation
        _forward = _cameraTransform.forward;
        _right = _cameraTransform.right;

        _forward.Normalize();
        _right.Normalize();

        // Movement vector from 2D to 3D from camera view
        _movement3D = _right * _movement2D.x + _forward * _movement2D.y;

        // Apply forces to movement vector
        _movement3D = new(_movement3D.x * _movementSpeed, // Apply movement speed
                         _verticalVelocity, // Apply vertical velocity
                         _movement3D.z * _movementSpeed); // Apply movement speed

        // Moves controller in the movement vector
        _playerManager._characterController.Move(Time.deltaTime * _movement3D);
    }

    /// <summary>
    /// Rotates player to face movement
    /// </summary>
    void HandleRotation()
    {
        // If there is any movement
        // To maintain rotation when stopping
        if (_movement2D != Vector2.zero)
        {
            // Rotate orientation
            Vector3 viewDir = _playerManager.transform.position - new Vector3(_cameraTransform.position.x, _playerManager.transform.position.y, _cameraTransform.position.z);
            _orientation.forward = viewDir.normalized;

            Vector3 inputDir = _orientation.forward * _movement2D.y + _orientation.right * _movement2D.x;

            _playerManager.transform.forward = Vector3.Slerp(_playerManager.transform.forward, inputDir.normalized, Time.deltaTime * _playerManager.rotationSpeed);
        }
    }
    #endregion
}
