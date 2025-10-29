using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// Defines a common class for all animation controllers.
/// Handles animation transitions.
/// </summary>
public class AAnimationController
{
    public readonly BehaviourController _behaviourController;
    readonly Animator _animator;
    readonly public int _idleAnim = Animator.StringToHash("Idle")
        , _walkAnim = Animator.StringToHash("Walk")
        , _runAnim = Animator.StringToHash("Run")
        , _preJumpAnim = Animator.StringToHash("PreJump")
        , _jumpAnim = Animator.StringToHash("Jump")
        , _afterJumpAnim = Animator.StringToHash("AfterJump")
        ;

    public int _currentAnimation, _lastAnimation;

    // Constructor
    public AAnimationController(BehaviourController behaviourController, Animator animator)
    {
        _behaviourController = behaviourController;
        _animator = animator;
    }

    #region PUBLIC METHODS
    /// <summary>
    /// Crossfade to new animation.
    /// </summary>
    public virtual void ChangeAnimationTo(int newAnimation, float duration = 0.2f)
    {
        // Not same as current
        if (_currentAnimation != newAnimation)
        {
            _lastAnimation = _currentAnimation;
            _currentAnimation = newAnimation;

            // Interpolate transition to new animation
            _animator.CrossFade(newAnimation, duration);
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
        // Get current animation state info
        AnimatorStateInfo currentStateInfo = _animator.GetCurrentAnimatorStateInfo(0);

        // If the animation is looping, it's never 'finished'
        if (currentStateInfo.loop)
            return false;

        // For non-looping animations, check if normalizedTime >= 1
        return currentStateInfo.normalizedTime >= 1f;
    }

    public bool IsAnimationFinished(int animation)
    {
        // Get current animation state info
        AnimatorStateInfo currentStateInfo = _animator.GetCurrentAnimatorStateInfo(0);

        // Given animation is the current one
        if (currentStateInfo.shortNameHash == animation)
            return IsCurrentAnimationFinished();
        else
            return false;
    }

    public void PlayAnimationCertainTime(float waitTime, int animation, string animationName, Action onComplete = null, bool showtext = true)
    {
        _behaviourController.StartCoroutine(PlayAnimationCertainTimeCoroutine(waitTime, animation, animationName, onComplete, showtext));
    }

    public void PlayAnimationRandomTime(int animation, string animationName, Action onComplete = null, bool showtext = true)
    {
        int waitTime = UnityEngine.Random.Range(5, 21);
        _behaviourController.StartCoroutine(PlayAnimationCertainTimeCoroutine(waitTime, animation, animationName, onComplete, showtext));
    }

    public IEnumerator PlayAnimationCertainTimeCoroutine(float waitTime, int animation, string animationName, Action onComplete = null, bool showtext = true)
    {
        if (_behaviourController._isExecutionPaused) yield break;
        _behaviourController._isExecutionPaused = true;

        if (showtext && waitTime >= 2f)
            ChangeAnimationTo(animation);
        yield return new WaitForSeconds(waitTime);

        _behaviourController._isExecutionPaused = false;
        onComplete?.Invoke();
    }
    #endregion
}
