using System;
using UnityEngine;

public class PlayableCharacterMovementController
{
    #region PROPERTY HELPERS
    float ArrivedHorizontalDistance => _player.ArrivedDistance.x;
    float ArrivedVerticalDistance => _player.ArrivedDistance.y;
    float NearHorizontalDistance => _player.NearDistance.x;
    float NearVerticalDistance => _player.NearDistance.y;
    #endregion

    #region PROPERTIES
    readonly PlayableCharacter _player;
    readonly CharacterController _playerCharacterController;
    readonly Transform _cameraTarget;

    float _verticalVelocity,
        _movementSpeed;

    bool _isRunPressed,
        _isJumpPressed,
        _isJumping;

    Vector3 _movement3DInput,
        _forward,
        _right;

    Vector2 _movementInput;
    #endregion

    #region CONSTRUCTOR
    public PlayableCharacterMovementController(PlayableCharacter player, CharacterController playerCharacterController)
    {
        _player = player;
        _playerCharacterController = playerCharacterController;
        _cameraTarget = CameraManager.Instance._thirdPersonCamera.LookAt;
    }
    #endregion

    #region INPUT MOVEMENT METHODS
    /// <summary>
    /// Updates the player movement,rotation and animations based on input actions.
    /// </summary>
    public void UpdateInputMovement()
    {
        // Prevent movement if jumping and grounded (pre-jump animation)
        if (_isJumping && _playerCharacterController.isGrounded)
            _movementInput = Vector2.zero;
        else
            _movementInput = GameManager.Instance.InputActions.Player.Move.ReadValue<Vector2>();

        _isRunPressed = GameManager.Instance.InputActions.Player.Sprint.IsPressed();
        _isJumpPressed = GameManager.Instance.InputActions.Player.Jump.IsPressed();

        // Get direction in 3D space based on camera orientation
        _forward = _cameraTarget.forward.normalized;
        _right = _cameraTarget.right.normalized;

        // Movement vector from 2D to 3D from camera orientation
        _movement3DInput = _right * _movementInput.x + _forward * _movementInput.y;
        _movement3DInput.y = 0f; // Prevent vertical rotation

        HandleInputGravity();
        HandleInputRotation();
        HandleInputAnimations();
        ApplyInputMovement();
    }

    /// <summary>
    /// Applies gravity to the player, handling jumping and falling
    /// </summary>
    void HandleInputGravity()
    {
        // Player on ground
        if (_playerCharacterController.isGrounded)
        {
            // Only apply jump force after pre-jump animation is finished
            if (_isJumping && _player.AnimationController.IsPreJumpAnimationFinished())
                _verticalVelocity = _player.JumpForce; // Jump
            else
                _verticalVelocity = -1f; // Small gravity to keep grounded
        }
        else
            // Apply gravity to vertical velocity
            _verticalVelocity -= _player.GravityForce * Time.deltaTime;
    }

    /// <summary>
    /// Rotates the player towards the movement direction based on camera orientation
    /// </summary>
    void HandleInputRotation()
    {
        // If there is any movement
        if (_movementInput != Vector2.zero)
            _player.transform.forward = Vector3.Slerp(_player.transform.forward, _movement3DInput.normalized, Time.deltaTime * _player.RotationSpeed);
    }

    void HandleInputAnimations()
    {
        if (_playerCharacterController.isGrounded)
        {
            // Handle jump sequence
            if (_isJumping)
            {
                if (_player.AnimationController.IsPreJumpAnimationFinished())
                {
                    _player.AnimationController.ChangeToJump();
                }
                else if (_player.AnimationController.IsJumpAnimationFinished())
                {
                    _player.AnimationController.ChangeToAfterJump();
                    _isJumping = false; // Reset jumping state after jump animation
                }
            }
            else if (_isJumpPressed)
            {
                _player.AnimationController.ChangeToPreJump();
                _isJumping = true;
            }
            else if (_movementInput == Vector2.zero)
                _player.AnimationController.ChangeToIdle();
            else if (_isRunPressed)
                _player.AnimationController.ChangeToRun();
            else
                _player.AnimationController.ChangeToWalk();
        }
        else // In air
        {
            // Play falling animation if not jumping
            // if (!_isJumping && _verticalVelocity < 0)
            // {
            //     _player.ChangeAnimationTo(_player._fallAnim);
            // }
        }
    }

    /// <summary>
    /// Applies movement to the player combining horizontal and vertical input.
    /// </summary>
    void ApplyInputMovement()
    {
        // Speed depending on sprint key
        _movementSpeed = _isRunPressed ? _player.SprintSpeed : _player.WalkSpeed;

        // Apply forces to movement vector
        Vector3 finalMovement = new(_movement3DInput.x * _movementSpeed, // Apply movement speed
                                    _verticalVelocity, // Apply vertical velocity
                                    _movement3DInput.z * _movementSpeed); // Apply movement speed

        // Moves controller in the movement vector
        _playerCharacterController.Move(Time.deltaTime * finalMovement);
    }
    #endregion

    #region NO INPUT MOVEMENT METHODS
    /// <summary>
    /// Moves the player towards the given destination position.
    /// </summary>
    /// <returns>True if the player has arrived at the destination, false otherwise.</returns>
    public bool SetDestination(Vector3 destinationPos)
    {
        Vector3 playerPos = _player.transform.position;

        // Horizontal distance on XZ plane
        Vector2 playerXZ = new(playerPos.x, playerPos.z);
        Vector2 destXZ = new(destinationPos.x, destinationPos.z);
        float horizontalDist = Vector2.Distance(playerXZ, destXZ);

        // Hasn't arrived
        if (horizontalDist > ArrivedHorizontalDistance)
        {
            // Calculate direction to destination
            Vector3 direction = (destinationPos - playerPos).normalized;

            // Apply forces to movement vector
            Vector3 finalMovement = new(direction.x * _player.WalkSpeed, // Apply walk speed
                                        _verticalVelocity, // Apply vertical velocity
                                        direction.z * _player.WalkSpeed); // Apply walk speed

            // Moves controller in the movement vector
            _playerCharacterController.Move(Time.deltaTime * finalMovement);

            // Walk animation
            _player.AnimationController.ChangeToWalk();

            return false;
        }
        // Has arrived
        else
        {
            // Idle animation
            _player.AnimationController.ChangeToIdle();

            return true;
        }
    }

    /// <summary>
    /// Smoothly rotates the player towards the given rotation
    /// </summary>
    public bool SmoothRotation(Quaternion rotation)
    {
        // Rotate player towards target rotation (just in XZ plane)
        Quaternion fixedXZRotation = Quaternion.Euler(0f, rotation.eulerAngles.y, 0f);
        _player.transform.rotation = Quaternion.Slerp(_player.transform.rotation, fixedXZRotation, Time.deltaTime * _player.RotationSpeed);

        // Check rotation difference
        float angleDifference = Quaternion.Angle(_player.transform.rotation, fixedXZRotation);
        return angleDifference < 0.1f;
    }
    #endregion
}
