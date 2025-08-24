using System;
using UnityEngine;

public class PlayerController
{
    #region PRIVATE PROPERTIES
    readonly PlayableCharacter _player;
    readonly CharacterController _playerCharacterController;
    readonly Transform _cameraTarget;

    float _verticalVelocity,
        _movementSpeed;

    bool _isRunPressed,
        _isJumpPressed,
        _isJumping;

    Vector3 _movement3D,
        _forward,
        _right;

    Vector2 _movementInput;
    #endregion


    // Constructor
    public PlayerController(PlayableCharacter player, CharacterController playerCharacterController)
    {
        _player = player;
        _playerCharacterController = playerCharacterController;
        _cameraTarget = CameraManager.Instance._thirdPersonCamera.LookAt;
    }

    #region PUBLIC METHODS
    public void Update()
    {
        // Prevent movement if jumping and grounded (pre-jump animation)
        if (_isJumping && _playerCharacterController.isGrounded)
            _movementInput = Vector2.zero;
        else
            _movementInput = GameManager.Instance._inputActions.Player.Move.ReadValue<Vector2>();

        _isRunPressed = GameManager.Instance._inputActions.Player.Sprint.IsPressed();
        _isJumpPressed = GameManager.Instance._inputActions.Player.Jump.IsPressed();

        // Get direction in 3D space based on camera orientation
        _forward = _cameraTarget.forward.normalized;
        _right = _cameraTarget.right.normalized;

        // Movement vector from 2D to 3D from camera orientation
        _movement3D = _right * _movementInput.x + _forward * _movementInput.y;
        _movement3D.y = 0f; // Prevent vertical rotation

        HandleGravity();
        HandleRotation();
        HandleAnimations();
        ApplyMovement();
    }
    #endregion

    #region PRIVATE METHODS

    /// <summary>
    /// Applies gravity to the player, handling jumping and falling
    /// </summary>
    void HandleGravity()
    {
        // Player on ground
        if (_playerCharacterController.isGrounded)
        {
            // Only apply jump force after pre-jump animation is finished
            if (_isJumping && _player._animationController.IsAnimationFinished(_player._animationController._preJumpAnim))
                _verticalVelocity = _player._jumpForce; // Jump
            else
                _verticalVelocity = -1f; // Small gravity to keep grounded
        }
        else
            // Apply gravity to vertical velocity
            _verticalVelocity -= _player._gravityForce * Time.deltaTime;
    }

    /// <summary>
    /// Rotates the player towards the movement direction based on camera orientation
    /// </summary>
    void HandleRotation()
    {
        // If there is any movement
        if (_movementInput != Vector2.zero)
            _player.transform.forward = Vector3.Slerp(_player.transform.forward, _movement3D.normalized, Time.deltaTime * _player._rotationSpeed);
    }

    void HandleAnimations()
    {
        if (_playerCharacterController.isGrounded)
        {
            // Handle jump sequence
            if (_isJumping)
            {
                if (_player._animationController.IsAnimationFinished(_player._animationController._preJumpAnim))
                {
                    _player._animationController.ChangeAnimationTo(_player._animationController._jumpAnim, 0f);
                }
                else if (_player._animationController.IsAnimationFinished(_player._animationController._jumpAnim))
                {
                    _player._animationController.ChangeAnimationTo(_player._animationController._afterJumpAnim, 0f);
                    _isJumping = false; // Reset jumping state after jump animation
                }
            }
            else if (_isJumpPressed)
            {
                _player._animationController.ChangeAnimationTo(_player._animationController._preJumpAnim, 0f);
                _isJumping = true;
            }
            else if (_movementInput == Vector2.zero)
                _player._animationController.ChangeAnimationTo(_player._animationController._idleAnim);
            else if (_isRunPressed)
                _player._animationController.ChangeAnimationTo(_player._animationController._runAnim);
            else
                _player._animationController.ChangeAnimationTo(_player._animationController._walkAnim);
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
    void ApplyMovement()
    {
        // Speed depending on sprint key
        _movementSpeed = _isRunPressed ? _player._runSpeed : _player._walkSpeed;

        // Apply forces to movement vector
        Vector3 finalMovement = new(_movement3D.x * _movementSpeed, // Apply movement speed
                                    _verticalVelocity, // Apply vertical velocity
                                    _movement3D.z * _movementSpeed); // Apply movement speed

        // Moves controller in the movement vector
        _playerCharacterController.Move(Time.deltaTime * finalMovement);
    }
    #endregion
}
