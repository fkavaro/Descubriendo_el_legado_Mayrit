using System;
using UnityEngine;

public class PlayableCharacterMovementController
{
    #region PROPERTIES
    readonly PlayableCharacter _player;
    readonly CharacterController _playerCharacterController;
    readonly Transform _cameraTarget;

    float _verticalVelocity;
    float _movementSpeed;

    bool _isRunPressed;
    bool _isJumpPressed;
    bool _isJumping;

    Vector3 _movement3DInput;
    Vector3 _forward;
    Vector3 _right;

    Vector2 _movementInput;
    bool _obstacleAhead;
    readonly int _obstacleMask;

    // Dependency Injection
    readonly CameraManager _cameraManager;
    readonly GameManager _gameManager;

    const float OBSTACLE_CHECK_DISTANCE = 0.25f;
    #endregion

    #region CONSTRUCTOR
    public PlayableCharacterMovementController(PlayableCharacter player, CharacterController playerCharacterController)
    {
        _player = player;
        _playerCharacterController = playerCharacterController;
        _obstacleMask = _player.ObstacleLayers.value;

        // Get dependencies from ServiceLocator
        _cameraManager = ServiceLocator.Instance.Get<CameraManager>();
        _gameManager = ServiceLocator.Instance.Get<GameManager>();

        _cameraTarget = _cameraManager.ThirdPersonCamera.LookAt;
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
            _movementInput = _gameManager.InputActions.Player.Move.ReadValue<Vector2>();

        _isRunPressed = _gameManager.InputActions.Player.Sprint.IsPressed();
        _isJumpPressed = _gameManager.InputActions.Player.Jump.WasPressedThisFrame();

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
            else if (_movementInput == Vector2.zero || _obstacleAhead)
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
        _movementSpeed = _isRunPressed ? _player.SprintSpeed : _player.WalkSpeed;

        Vector3 horizontalInput = ComputeHorizontalInput();
        _obstacleAhead = ResolveObstacles(horizontalInput, out Vector3 resolvedHorizontal);

        Vector3 finalMovement = new(resolvedHorizontal.x * _movementSpeed,
                                    _verticalVelocity,
                                    resolvedHorizontal.z * _movementSpeed);

        _playerCharacterController.Move(Time.deltaTime * finalMovement);
    }

    Vector3 ComputeHorizontalInput()
    {
        return new Vector3(_movement3DInput.x, 0f, _movement3DInput.z);
    }

    bool ResolveObstacles(Vector3 horizontalInput, out Vector3 resolved)
    {
        bool hit = horizontalInput != Vector3.zero && IsObstacleAhead(horizontalInput);
        resolved = hit ? Vector3.zero : horizontalInput;
        return hit;
    }

    bool IsObstacleAhead(Vector3 movementDirection)
    {
        // Normalize intended horizontal movement (Y already zeroed before calling)
        Vector3 direction = movementDirection.normalized;

        // Build a capsule that matches the CharacterController volume
        Vector3 center = _playerCharacterController.bounds.center;
        float radius = _playerCharacterController.radius - _playerCharacterController.skinWidth;
        float height = Mathf.Max(_playerCharacterController.height, radius * 2f);
        Vector3 top = center + Vector3.up * (height * 0.5f - radius);  // top sphere center
        // Raise the bottom point to center height to avoid ground/terrain hits while still detecting obstacles
        Vector3 bottom = center; // middle sphere center

        // Cast forward a short distance; ignore triggers; only test configured obstacle layers
        // Use a distance at least as large as the controller radius to avoid missing nearby colliders
        float checkDistance = Mathf.Max(OBSTACLE_CHECK_DISTANCE, radius + _playerCharacterController.skinWidth);

        return Physics.CapsuleCast(top,
                                    bottom,
                                    radius,
                                    direction,
                                    out RaycastHit _,
                                    checkDistance,
                                    _obstacleMask,
                                    QueryTriggerInteraction.Ignore);
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
        if (horizontalDist > _player.ArrivingDistance)
        {
            // Calculate direction to destination (XZ plane only)
            Vector3 direction = (destinationPos - playerPos).normalized;
            direction.y = 0f;  // Only rotate around vertical axis
            direction.Normalize();

            // Rotate player to face destination
            _player.transform.forward = Vector3.Slerp(_player.transform.forward, direction, Time.deltaTime * _player.RotationSpeed);

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

    public void ApplyGravity()
    {
        // Player on ground
        if (_playerCharacterController.isGrounded)
            _verticalVelocity = -1f; // Small gravity to keep grounded
        else
            // Apply gravity to vertical velocity
            _verticalVelocity -= _player.GravityForce * Time.deltaTime;

        // Apply forces to movement vector
        Vector3 finalMovement = new(0f, _verticalVelocity, 0f);

        // Moves controller in the movement vector
        _playerCharacterController.Move(Time.deltaTime * finalMovement);
    }
    #endregion
}
