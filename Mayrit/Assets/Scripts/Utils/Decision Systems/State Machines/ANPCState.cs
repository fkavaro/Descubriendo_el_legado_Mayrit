using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Base class for NPC states, allowing to handle animations.
/// </summary>
public abstract class ANPCState<TController, TStateMachine> : AState<TController, TStateMachine>
    where TController : MonoBehaviour
    where TStateMachine : AStateMachine<TController, TStateMachine>
{
    readonly ANPC<TController> _npc;

    public ANPCState(string name, TStateMachine stateMachine, ANPC<TController> npc)
    : base(name, stateMachine)
    {
        _npc = npc;
    }

    /// <summary>
    /// Switchs to the next state afer random time playing an animation.
    /// </summary>
    protected void SwitchStateAfterRandomTime(AState<TController, TStateMachine> nextState, int animation, string animationName)
    {
        int waitTime = Random.Range(5, 21);
        // TODO: MAKE THIS WORK
        //_controller.StartCoroutine(SwitchStateAfterCertainTimeCoroutine(waitTime, nextState, animation, animationName));
    }

    /// <summary>
    /// Switchs to the next state afer given time playing an animation.
    /// </summary>
    protected void SwitchStateAfterCertainTime(float waitTime, AState<TController, TStateMachine> nextState, int animation, string animationName)
    {
        // TODO: MAKE THIS WORK
        //_controller.StartCoroutine(SwitchStateAfterCertainTimeCoroutine(waitTime, nextState, animation, animationName));
    }

    // IEnumerator SwitchStateAfterCertainTimeCoroutine(float waitTime, AState<TController, TStateMachine> nextState, int animation, string animationName)
    // {
    //     TODO: MAKE THIS WORK
    //     //yield return _controller.PlayAnimationCertainTimeCoroutine(waitTime, animation, animationName);
    //     SwitchState(nextState);
    // }

    /// <summary>
    /// Switches to the next state if the animation is finished.
    /// </summary>
    protected void SwitchStateAfterAnimation(AState<TController, TStateMachine> nextState, int animation = -1)
    {
        if (animation != -1)
            _npc.ChangeAnimationTo(animation);

        if (_npc.IsCurrentAnimationFinished())
            SwitchState(nextState);
    }
}
