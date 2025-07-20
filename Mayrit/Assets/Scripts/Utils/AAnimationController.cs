using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// Defines a common class for all animation controllers.
/// Handles animation transitions.
/// </summary>
[RequireComponent(typeof(Animator))]
public abstract class AAnimationController<TController> : ABehaviourController<TController>
where TController : ABehaviourController<TController>
{
    protected Animator animator;
    protected int currentAnimation, lastAnimation;

    #region COMMON ANIMATIONS
    readonly public int idleAnim = Animator.StringToHash("Idle"),
        walkAnim = Animator.StringToHash("Walk");
    #endregion
    // Specific animations must be defined in derived classes

    #region INHERITED METHODS
    protected override void OnAwake()
    {
        animator = GetComponent<Animator>();
    }
    #endregion

    #region PUBLIC METHODS
    /// <summary>
    /// Crossfade to new animation.
    /// </summary>
    public virtual void ChangeAnimationTo(int newAnimation, float duration = 0.2f)
    {
        // Not same as current
        if (currentAnimation != newAnimation)
        {
            lastAnimation = currentAnimation;
            currentAnimation = newAnimation;

            // Interpolate transition to new animation
            animator.CrossFade(newAnimation, duration);
        }
    }

    /// <summary>
    /// Crossfade to previous animation.
    /// </summary>
    public virtual void ChangeToPreviousAnimation(float duration = 0.2f)
    {
        ChangeAnimationTo(lastAnimation, duration);
    }

    /// <returns> True if the current animation is finished, false otherwise.</returns>
    public virtual bool IsAnimationFinished()
    {
        // Check if the current animation is finished
        AnimatorStateInfo currentStateInfo = animator.GetCurrentAnimatorStateInfo(0);

        // If the animation is looping, it's never 'finished'
        if (currentStateInfo.loop)
        {
            //Debug.LogWarning("Loop animation wont't finish");
            return false;
        }

        // For non-looping animations, check if normalizedTime >= 1
        return currentStateInfo.normalizedTime >= 1f;
    }

    public void PlayAnimationCertainTime(float waitTime, int animation, string animationName, Action onComplete = null, bool showtext = true)
    {
        StartCoroutine(PlayAnimationCertainTimeCoroutine(waitTime, animation, animationName, onComplete, showtext));
    }

    public void PlayAnimationRandomTime(int animation, string animationName, Action onComplete = null, bool showtext = true)
    {
        int waitTime = UnityEngine.Random.Range(5, 21);
        StartCoroutine(PlayAnimationCertainTimeCoroutine(waitTime, animation, animationName, onComplete, showtext));
    }

    public IEnumerator PlayAnimationCertainTimeCoroutine(float waitTime, int animation, string animationName, Action onComplete = null, bool showtext = true)
    {
        if (isExecutionPaused) yield break;

        isExecutionPaused = true;

        if (showtext && waitTime >= 2f)
            animationText.text = animationName + " for " + waitTime + " seconds...";

        ChangeAnimationTo(animation);

        yield return new WaitForSeconds(waitTime);

        animationText.text = "";
        isExecutionPaused = false;
        onComplete?.Invoke();
    }
    #endregion
}
