using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// Controller for animation handling via Animator component.
/// </summary>
public class CharacterAnimationController
{
    #region PROPERTIES
    readonly ICharacter _character;
    readonly IBehaviourEntity _behaviourEntity;
    readonly Animator _animator;

    readonly int _idleAnim = Animator.StringToHash("Idle")
        , _walkAnim = Animator.StringToHash("Walk")
        , _runAnim = Animator.StringToHash("Run")
        ;

    int _lastAnimation, _currentAnimation;
    #endregion

    #region CONSTRUCTOR
    public CharacterAnimationController(ICharacter character, IBehaviourEntity behaviourEntity, Animator animator)
    {
        _character = character;
        _behaviourEntity = behaviourEntity;
        _animator = animator;
    }
    #endregion

    #region PUBLIC METHODS
    public void TryFixCurrentAnimation()
    {
        if (IsAnimatorAvailable() && !IsCurrentAnimation(_currentAnimation))
        {
            ChangeAnimationTo(_currentAnimation);

            if (_behaviourEntity.DebugMode)
                Debug.Log($"[{_behaviourEntity.Name}.AnimationController.TryFixCurrentAnimation()] Current animation was not active. Fixed by reapplying it.", _character.CharacterModel);
        }
    }

    public void Reset()
    {
        _lastAnimation = -1;
        _currentAnimation = -1;
    }

    /// <summary>
    /// Crossfade to new animation.
    /// </summary>
    public virtual void ChangeAnimationTo(int requestedAnimation, float duration = 0.2f)
    {
        if (!IsAnimatorAvailable())
        {
            if (_behaviourEntity.DebugMode)
                Debug.LogWarning($"[AnimationController.ChangeAnimationTo()] {_behaviourEntity.Name}: Animator not available", _behaviourEntity.GO);
            return;
        }

        // Ensure the animator actually contains the requested state. If not, fall back to idle.
        if (requestedAnimation <= 0 || !_animator.HasState(0, requestedAnimation))
        {
            if (_behaviourEntity.DebugMode)
                Debug.LogWarning($"[AnimationController.ChangeAnimationTo()] {_behaviourEntity.Name}: Requested animation state not found in Animator. Falling back to Idle.", _behaviourEntity.GO);

            requestedAnimation = _idleAnim;

            if (!_animator.HasState(0, requestedAnimation))
            {
                if (_behaviourEntity.DebugMode)
                    Debug.LogWarning($"[AnimationController.ChangeAnimationTo()] {_behaviourEntity.Name}: Idle state also missing. Aborting animation change.", _behaviourEntity.GO);
                return;
            }
        }

        _currentAnimation = requestedAnimation;

        // Determine effective state (current or next when in transition)
        int effectiveState = GetEffectiveStateHash(0);

        // Change to requested animation if it isn't already active or transitioning to it
        if (effectiveState != requestedAnimation)
        {
            // Update bookkeeping with current state's shortNameHash
            _lastAnimation = _animator.GetCurrentAnimatorStateInfo(0).shortNameHash;

            // Crossfade to the new animation on base layer (layer 0)
            _animator.CrossFade(requestedAnimation, duration, 0);
        }
    }

    /// <summary>
    /// Crossfade to previous animation.
    /// </summary>
    public virtual void ChangeToPreviousAnimation(float duration = 0.2f)
    {
        ChangeAnimationTo(_lastAnimation, duration);
    }

    /// <returns> True if the current animation is finished, false otherwise.</returns>
    public virtual bool IsCurrentAnimationFinished()
    {
        if (!IsAnimatorAvailable())
            return true;

        // Get current animation state info
        AnimatorStateInfo currentStateInfo = _animator.GetCurrentAnimatorStateInfo(0);

        // If the animation is looping, it's never 'finished'
        if (currentStateInfo.loop)
            return false;

        // For non-looping animations, check if normalizedTime >= 1
        return currentStateInfo.normalizedTime >= 1f;
    }

    public void PlayAnimationCertainTime(float waitTime, int animation, string animationName, Action onComplete = null)
    {
        _character.StartCoroutine(PlayAnimationCertainTimeCoroutine(waitTime, animation, animationName, onComplete));
    }

    public void PlayAnimationRandomTime(int animation, string animationName, Action onComplete = null)
    {
        int waitTime = UnityEngine.Random.Range(5, 21);
        _character.StartCoroutine(PlayAnimationCertainTimeCoroutine(waitTime, animation, animationName, onComplete));
    }

    public IEnumerator PlayAnimationCertainTimeCoroutine(float waitTime, int animation, string animationName, Action onComplete = null)
    {
        if (_behaviourEntity.IsExecutionPaused) yield break;
        _behaviourEntity.IsExecutionPaused = true;

        if (waitTime >= 2f)
            ChangeAnimationTo(animation);
        yield return new WaitForSeconds(waitTime);

        _behaviourEntity.IsExecutionPaused = false;
        onComplete?.Invoke();
    }
    #endregion

    #region PRIVATE METHODS
    bool IsAnimatorAvailable()
    {
        return _animator != null && _animator.isActiveAndEnabled && _character.CharacterModel.activeInHierarchy;
    }

    bool IsCurrentAnimation(int animation)
    {
        if (!IsAnimatorAvailable())
            return false;

        // Compare requested animation against effective state (handles transitions)
        return GetEffectiveStateHash(0) == animation;
    }

    bool IsAnimationFinished(int animation)
    {
        if (!IsAnimatorAvailable())
            return true;

        if (!IsCurrentAnimation(animation))
            return false;

        return IsCurrentAnimationFinished();
    }

    // Returns the active state's shortNameHash, considering ongoing transitions
    int GetEffectiveStateHash(int layer)
    {
        if (!IsAnimatorAvailable())
            return 0;

        if (_animator.IsInTransition(layer))
        {
            AnimatorStateInfo next = _animator.GetNextAnimatorStateInfo(layer);
            if (next.shortNameHash != 0)
                return next.shortNameHash;
        }
        return _animator.GetCurrentAnimatorStateInfo(layer).shortNameHash;
    }
    #endregion

    #region HELPER PUBLIC METHODS
    public void ChangeToIdle() => ChangeAnimationTo(_idleAnim);

    public void ChangeToWalk() => ChangeAnimationTo(_walkAnim);

    public void ChangeToRun() => ChangeAnimationTo(_runAnim);
    //|||||||||||||||||||||||||||||||||||||||||
    public bool IsIdleAnimationFinished => IsAnimationFinished(_idleAnim);

    public bool IsWalkAnimationFinished => IsAnimationFinished(_walkAnim);

    public bool IsRunAnimationFinished => IsAnimationFinished(_runAnim);
    //|||||||||||||||||||||||||||||||||||||||||
    public bool IsIdling => IsCurrentAnimation(_idleAnim);

    public bool IsWalking => IsCurrentAnimation(_walkAnim);

    public bool IsRunning => IsCurrentAnimation(_runAnim);
    #endregion
}
