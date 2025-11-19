using UnityEngine;
using System.Collections;

/// <summary>
/// Base class for NPC states, allowing to handle animations.
/// </summary>
public abstract class ANPCState<TStateMachine> : AState<TStateMachine>
where TStateMachine : AStateMachine<TStateMachine>
{
    readonly ANPC<TStateMachine> _npc;

    public ANPCState(string stateName, TStateMachine stateMachine, ANPC<TStateMachine> npc)
    : base(stateName, stateMachine)
    {
        _npc = npc;
    }

    /// <summary>
    /// Switchs to the next state afer random time playing an animation.
    /// </summary>
    protected void SwitchStateAfterRandomTime(AState<TStateMachine> nextState, int animation, string animationName)
    {
        int waitTime = Random.Range(5, 21);

        _npc.StartCoroutine(SwitchStateAfterCertainTimeCoroutine(waitTime, nextState, animation, animationName));
    }

    /// <summary>
    /// Switchs to the next state afer given time playing an animation.
    /// </summary>
    protected void SwitchStateAfterCertainTime(float waitTime, AState<TStateMachine> nextState, int animation, string animationName)
    {
        _npc.StartCoroutine(SwitchStateAfterCertainTimeCoroutine(waitTime, nextState, animation, animationName));
    }

    IEnumerator SwitchStateAfterCertainTimeCoroutine(float waitTime, AState<TStateMachine> nextState, int animation, string animationName)
    {
        yield return _npc.AnimationController.PlayAnimationCertainTimeCoroutine(waitTime, animation, animationName);
        SwitchState(nextState);
    }

    /// <summary>
    /// Switches to the next state if the animation is finished.
    /// </summary>
    protected void SwitchStateAfterAnimation(AState<TStateMachine> nextState, int animation = -1)
    {
        if (animation != -1)
            _npc.AnimationController.ChangeAnimationTo(animation);

        if (_npc.AnimationController.IsCurrentAnimationFinished())
            SwitchState(nextState);
    }
}
