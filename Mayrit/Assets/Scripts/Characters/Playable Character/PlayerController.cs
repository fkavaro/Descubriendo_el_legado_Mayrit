using System;
using UnityEngine;

public class PlayerController
{
    #region PRIVATE PROPERTIES
    readonly PlayableCharacter _player;
    readonly CharacterController _playerCharacterController;
    readonly Transform _cameraTransform,
        _cameraOrientationFollower;

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
    #endregion


    // Constructor
    public PlayerController(PlayableCharacter player, CharacterController playerCharacterController)
    {
        _player = player;
        _playerCharacterController = playerCharacterController;
        _cameraTransform = CameraManager.Instance._thirdPersonCamera.transform;
        _cameraOrientationFollower = _player._cameraOrientationFollower;
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

        HandleMovement();
        HandleGravity();
        HandleRotation();
        HandleAnimations();
        ApplyMovement();
    }
    #endregion

    #region PRIVATE METHODS
    /// <summary>
    /// Handles player movement based on input and camera orientation.
    /// </summary>
    void HandleMovement()
    {
        // Sprint key pressed
        _movementSpeed = _isRunPressed ? _player._runSpeed : _player._walkSpeed;

        // Get direction in 3D space based on camera orientation
        _forward = _cameraTransform.forward.normalized;
        _right = _cameraTransform.right.normalized;
    }

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
        {
            // Rotate orientation
            Vector3 cameraPos = new(_cameraTransform.position.x, _player.transform.position.y, _cameraTransform.position.z);
            Vector3 viewDir = _player.transform.position - cameraPos;
            _cameraOrientationFollower.forward = viewDir.normalized;

            Vector3 inputDir = _cameraOrientationFollower.forward * _movementInput.y + _cameraOrientationFollower.right * _movementInput.x;
            inputDir.y = 0f; // Prevent vertical rotation

            // Faster rotation if walking
            _rotationSpeed = _isRunPressed ? _player._rotationSpeed : _player._rotationSpeed * 2f;

            _player.transform.forward = Vector3.Slerp(_player.transform.forward, inputDir.normalized, Time.deltaTime * _rotationSpeed);
        }

        // // Only rotate if there is movement input
        // if (_movementInput != Vector2.zero)
        // {
        //     // Calculate direction based on camera orientation and input
        //     Vector3 inputDir = _cameraOrientationFollower.forward * _movementInput.y + _cameraOrientationFollower.right * _movementInput.x;
        //     inputDir.y = 0f; // Prevent vertical rotation

        //     if (inputDir.sqrMagnitude > 0.001f)
        //     {
        //         // Set rotation speed: running rotates faster
        //         _rotationSpeed = _isRunPressed ? _player._rotationSpeed * 2f : _player._rotationSpeed;

        //         // Smoothly rotate player towards movement direction
        //         Quaternion targetRotation = Quaternion.LookRotation(inputDir.normalized);
        //         _player.transform.rotation = Quaternion.Slerp(_player.transform.rotation, targetRotation, Time.deltaTime * _rotationSpeed);

        //         // Update orientation to match player forward
        //         _cameraOrientationFollower.forward = _player.transform.forward;
        //     }
        // }
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
        // Movement vector from 2D to 3D from camera view
        _movement3D = _right * _movementInput.x + _forward * _movementInput.y;

        // Apply forces to movement vector
        _movement3D = new(_movement3D.x * _movementSpeed, // Apply movement speed
                         _verticalVelocity, // Apply vertical velocity
                         _movement3D.z * _movementSpeed); // Apply movement speed

        // Moves controller in the movement vector
        _playerCharacterController.Move(Time.deltaTime * _movement3D);
    }
    #endregion
}
