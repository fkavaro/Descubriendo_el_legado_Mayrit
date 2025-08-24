using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// Defines a common class for all animation controllers.
/// Handles animation transitions.
/// </summary>
public class AAnimationController<TController> : ABehaviourController<TController>
where TController : MonoBehaviour
{
    readonly Animator _animator;
    public int _currentAnimation, _lastAnimation;

    #region COMMON ANIMATIONS
    readonly public int _idleAnim = Animator.StringToHash("Idle")
        , _walkAnim = Animator.StringToHash("Walk")
        , _runAnim = Animator.StringToHash("Run")
        , _preJumpAnim = Animator.StringToHash("PreJump")
        , _jumpAnim = Animator.StringToHash("Jump")
        , _afterJumpAnim = Animator.StringToHash("AfterJump")
        ;
    #endregion
    // Constructor
    public AAnimationController(string name, Animator animator)
    : base(name)
    {
        _animator = animator;
    }

    #region INHERITED METHODS
    #endregion

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
        // TODO: MAKE THIS WORK
        //StartCoroutine(PlayAnimationCertainTimeCoroutine(waitTime, animation, animationName, onComplete, showtext));
    }

    public void PlayAnimationRandomTime(int animation, string animationName, Action onComplete = null, bool showtext = true)
    {
        int waitTime = UnityEngine.Random.Range(5, 21);
        // TODO: MAKE THIS WORK
        //StartCoroutine(PlayAnimationCertainTimeCoroutine(waitTime, animation, animationName, onComplete, showtext));
    }

    public IEnumerator PlayAnimationCertainTimeCoroutine(float waitTime, int animation, string animationName, Action onComplete = null, bool showtext = true)
    {
        if (_isExecutionPaused) yield break;
        _isExecutionPaused = true;

        if (showtext && waitTime >= 2f)
            ChangeAnimationTo(animation);
        yield return new WaitForSeconds(waitTime);

        _isExecutionPaused = false;
        onComplete?.Invoke();
    }
    #endregion
}
