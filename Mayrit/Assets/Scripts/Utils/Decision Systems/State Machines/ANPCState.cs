using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Base class for NPC states, allowing to handle animations.
/// </summary>
public abstract class ANPCState<TController, TStateMachine> : AState<TController, TStateMachine>
    where TController : ANPC<TController>
    where TStateMachine : AStateMachine<TController, TStateMachine>
{
    public ANPCState(string name, TStateMachine stateMachine) : base(name, stateMachine) { }

    /// <summary>
    /// Switchs to the next state afer random time playing an animation.
    /// </summary>
    protected void SwitchStateAfterRandomTime(AState<TController, TStateMachine> nextState, int animation, string animationName)
    {
        int waitTime = Random.Range(5, 21);
        _controller.StartCoroutine(SwitchStateAfterCertainTimeCoroutine(waitTime, nextState, animation, animationName));
    }

    /// <summary>
    /// Switchs to the next state afer given time playing an animation.
    /// </summary>
    protected void SwitchStateAfterCertainTime(float waitTime, AState<TController, TStateMachine> nextState, int animation, string animationName)
    {
        _controller.StartCoroutine(SwitchStateAfterCertainTimeCoroutine(waitTime, nextState, animation, animationName));
    }

    IEnumerator SwitchStateAfterCertainTimeCoroutine(float waitTime, AState<TController, TStateMachine> nextState, int animation, string animationName)
    {
        yield return _controller.PlayAnimationCertainTimeCoroutine(waitTime, animation, animationName);
        SwitchState(nextState);
    }

    /// <summary>
    /// Switches to the next state if the animation is finished.
    /// </summary>
    protected void SwitchStateAfterAnimation(AState<TController, TStateMachine> nextState, int animation = -1)
    {
        if (animation != -1)
            _controller.ChangeAnimationTo(animation);

        if (_controller.IsAnimationFinished())
            SwitchState(nextState);
    }
}
