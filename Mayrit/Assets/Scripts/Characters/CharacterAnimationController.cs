using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// Controller for animation handling via Animator component.
/// </summary>
public class CharacterAnimationController
{
    #region PROPERTIES
    readonly MonoBehaviour _entity;
    readonly IBehaviourEntity _behaviourEntity;
    readonly Animator _animator;

    readonly int _idleAnim = Animator.StringToHash("Idle")
        , _walkAnim = Animator.StringToHash("Walk")
        , _runAnim = Animator.StringToHash("Run")
        , _preJumpAnim = Animator.StringToHash("PreJump")
        , _jumpAnim = Animator.StringToHash("Jump")
        , _afterJumpAnim = Animator.StringToHash("AfterJump")
        , _talkAnim = Animator.StringToHash("Talk") // TODO: import talk animation
        ;

    int _lastPlayedAnimation;
    #endregion

    #region CONSTRUCTOR
    public CharacterAnimationController(MonoBehaviour entity, IBehaviourEntity behaviourEntity, Animator animator)
    {
        _entity = entity;
        _behaviourEntity = behaviourEntity;
        _animator = animator;
    }
    #endregion

    #region PUBLIC METHODS
    public void Reset()
    {
        _lastPlayedAnimation = -1;
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
        }

        // Determine effective state (current or next when in transition)
        int effectiveState = GetEffectiveStateHash(0);

        // Change to requested animation if it isn't already active or transitioning to it
        if (effectiveState != requestedAnimation)
        {
            // Update bookkeeping with current state's shortNameHash
            _lastPlayedAnimation = _animator.GetCurrentAnimatorStateInfo(0).shortNameHash;

            // Crossfade to the new animation on base layer (layer 0)
            _animator.CrossFade(requestedAnimation, duration, 0);
        }
    }

    /// <summary>
    /// Crossfade to previous animation.
    /// </summary>
    public virtual void ChangeToPreviousAnimation(float duration = 0.2f)
    {
        ChangeAnimationTo(_lastPlayedAnimation, duration);
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
        _entity.StartCoroutine(PlayAnimationCertainTimeCoroutine(waitTime, animation, animationName, onComplete));
    }

    public void PlayAnimationRandomTime(int animation, string animationName, Action onComplete = null)
    {
        int waitTime = UnityEngine.Random.Range(5, 21);
        _entity.StartCoroutine(PlayAnimationCertainTimeCoroutine(waitTime, animation, animationName, onComplete));
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
        return _animator != null && _animator.isActiveAndEnabled;
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
    public void ChangeToIdle()
    {
        ChangeAnimationTo(_idleAnim);
    }

    public void ChangeToWalk()
    {
        ChangeAnimationTo(_walkAnim);
    }

    public void ChangeToRun()
    {
        ChangeAnimationTo(_runAnim);
    }

    public void ChangeToPreJump()
    {
        ChangeAnimationTo(_preJumpAnim);
    }

    public void ChangeToJump()
    {
        ChangeAnimationTo(_jumpAnim);
    }

    public void ChangeToAfterJump()
    {
        ChangeAnimationTo(_afterJumpAnim);
    }

    public void ChangeToTalk()
    {
        ChangeAnimationTo(_talkAnim);
    }
    //|||||||||||||||||||||||||||||||||||||||||
    public bool IsIdleAnimationFinished()
    {
        return IsAnimationFinished(_idleAnim);
    }

    public bool IsWalkAnimationFinished()
    {
        return IsAnimationFinished(_walkAnim);
    }

    public bool IsRunAnimationFinished()
    {
        return IsAnimationFinished(_runAnim);
    }

    public bool IsPreJumpAnimationFinished()
    {
        return IsAnimationFinished(_preJumpAnim);
    }

    public bool IsJumpAnimationFinished()
    {
        return IsAnimationFinished(_jumpAnim);
    }

    public bool IsAfterJumpAnimationFinished()
    {
        return IsAnimationFinished(_afterJumpAnim);
    }

    public bool IsTalkAnimationFinished()
    {
        return IsAnimationFinished(_idleAnim); // TODO: talk animation
    }
    //|||||||||||||||||||||||||||||||||||||||||
    public bool IsIdling()
    {
        return IsCurrentAnimation(_idleAnim);
    }

    public bool IsWalking()
    {
        return IsCurrentAnimation(_walkAnim);
    }

    public bool IsRunning()
    {
        return IsCurrentAnimation(_runAnim);
    }

    public bool IsPreJumping()
    {
        return IsCurrentAnimation(_preJumpAnim);
    }

    public bool IsJumping()
    {
        return IsCurrentAnimation(_jumpAnim);
    }

    public bool IsAfterJumping()
    {
        return IsCurrentAnimation(_afterJumpAnim);
    }

    public bool IsTalking()
    {
        return IsCurrentAnimation(_talkAnim);
    }
    #endregion
}
