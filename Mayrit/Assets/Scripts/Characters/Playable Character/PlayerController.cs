using System;
using UnityEngine;

public class PlayerController
{
    #region PRIVATE PROPERTIES
    readonly PlayableCharacter _player;

    float _verticalVelocity,
        _movementSpeed,
        _rotationSpeed;

    bool _isRunPressed,
        _isJumpPressed,
        _isJumping;

    Vector3 _movement3D,
        _forward,
        _right;

    Vector2 _movementInput;

    Transform _cameraTransform,
        _orientation;
    #endregion

    #region PUBLIC METHODS
    public PlayerController(PlayableCharacter player)
    {
        _player = player;
    }

    public void Start()
    {
        _cameraTransform = Camera.main.transform;
        _orientation = _player._orientation;
    }

    public void Update()
    {
        // Read input actions from GameManager
        _movementInput = GameManager.Instance._inputActions.Player.Move.ReadValue<Vector2>();
        _isRunPressed = GameManager.Instance._inputActions.Player.Sprint.IsPressed();
        _isJumpPressed = GameManager.Instance._inputActions.Player.Jump.IsPressed();

        HandleAnimations();
        HandleMovement();
        HandleGravity();
        HandleRotation();
        ApplyMovement();
    }


    #endregion

    #region PRIVATE METHODS
    void HandleAnimations()
    {
        if (_player._characterController.isGrounded)
        {
            // Jumping
            if (_isJumping)
            {
                if (_player.IsAnimationFinished(_player._preJumpAnim))
                {
                    _player.ChangeAnimationTo(_player._jumpAnim, 0f);
                }
                else if (_player.IsAnimationFinished(_player._jumpAnim))
                {
                    _player.ChangeAnimationTo(_player._afterJumpAnim, 0f);
                    _isJumping = false; // Reset jumping state after jump animation
                }
            }
            // Not jumping
            else
            {
                if (_isJumpPressed)
                {
                    _player.ChangeAnimationTo(_player._preJumpAnim, 0f);
                    _isJumping = true;
                }
                else if (_movementInput == Vector2.zero)
                    _player.ChangeAnimationTo(_player._idleAnim);
                else if (_isRunPressed)
                    _player.ChangeAnimationTo(_player._runAnim);
                else
                    _player.ChangeAnimationTo(_player._walkAnim);
            }
        }
    }

    /// <summary>
    /// Handles player movement based on input and camera orientation.
    /// </summary>
    void HandleMovement()
    {
        // Don't apply movement if jumping but still on ground (pre-jump animation)
        if (_isJumping && _player._characterController.isGrounded)
        {
            _movementInput = Vector2.zero;
            return;
        }

        // Sprint key pressed
        if (_isRunPressed)
            _movementSpeed = _player._runSpeed; // Move with sprint speed
                                                // Not pressed
        else
            _movementSpeed = _player._walkSpeed; // Move with walk speed

        // Get direction in 3D space based on camera orientation
        _forward = _cameraTransform.forward;
        _right = _cameraTransform.right;

        _forward.Normalize();
        _right.Normalize();
    }

    /// <summary>
    /// Applies gravity to the player, handling jumping and falling
    /// </summary>
    void HandleGravity()
    {
        // Player on ground
        if (_player._characterController.isGrounded)
        {
            // Pre-jump animation finished
            if (_player.IsAnimationFinished(_player._preJumpAnim))
                _verticalVelocity = _player._jumpForce; // Jump
            // Not finished
            else
                _verticalVelocity = -1f; // Small gravity to keep grounded
        }
        // Not on ground
        else
        {
            // Apply gravity to vertical velocity
            _verticalVelocity -= _player._gravityForce * Time.deltaTime;
        }
    }

    /// <summary>
    /// Rotates the player towards the movement direction based on camera orientation
    /// </summary>
    void HandleRotation()
    {
        // If there is any movement
        // To maintain rotation when stopping
        if (_movementInput != Vector2.zero)
        {
            // Rotate orientation
            Vector3 viewDir = _player.transform.position - new Vector3(_cameraTransform.position.x, _player.transform.position.y, _cameraTransform.position.z);
            _orientation.forward = viewDir.normalized;

            Vector3 inputDir = _orientation.forward * _movementInput.y + _orientation.right * _movementInput.x;

            // Faster rotation if walking
            if (!_isRunPressed)
                _rotationSpeed = _player._rotationSpeed;
            else
                _rotationSpeed = _player._rotationSpeed * 2f;

            _player.transform.forward = Vector3.Slerp(_player.transform.forward, inputDir.normalized, Time.deltaTime * _rotationSpeed);
        }
    }

    /// <summary>
    /// Applies movement to the player combining horizontal and vertical input.
    /// </summary>
    void ApplyMovement()
    {
        // Movement vector from 2D to 3D from camera view
        _movement3D = _right * _movementInput.x + _forward * _movementInput.y;

        // Apply forces to movement vector
        _movement3D = new(_movement3D.x * _movementSpeed, // Apply movement speed
                         _verticalVelocity, // Apply vertical velocity
                         _movement3D.z * _movementSpeed); // Apply movement speed

        // Moves controller in the movement vector
        _player._characterController.Move(Time.deltaTime * _movement3D);
    }
    #endregion
}
