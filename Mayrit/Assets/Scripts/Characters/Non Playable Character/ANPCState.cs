using UnityEngine;
using System.Collections;

/// <summary>
/// Base class for NPC states, allowing to handle animations.
/// </summary>
// TODO: remove if not used
public abstract class ANPCState : AState
{
    readonly ANPC<AStateMachine<ANPCState>> _npc;

    public ANPCState(string stateName, ANPC<AStateMachine<ANPCState>> npc)
    : base(stateName)
    {
        _npc = npc;
    }

    /// <summary>
    /// Switchs to the next state afer random time playing an animation.
    /// </summary>
    protected void SwitchStateAfterRandomTime(ANPCState nextState, int animation, string animationName)
    {
        int waitTime = Random.Range(5, 21);

        _npc.StartCoroutine(SwitchStateAfterCertainTimeCoroutine(waitTime, nextState, animation, animationName));
    }

    /// <summary>
    /// Switchs to the next state afer given time playing an animation.
    /// </summary>
    protected void SwitchStateAfterCertainTime(float waitTime, ANPCState nextState, int animation, string animationName)
    {
        _npc.StartCoroutine(SwitchStateAfterCertainTimeCoroutine(waitTime, nextState, animation, animationName));
    }

    IEnumerator SwitchStateAfterCertainTimeCoroutine(float waitTime, ANPCState nextState, int animation, string animationName)
    {
        yield return _npc.AnimationController.PlayAnimationCertainTimeCoroutine(waitTime, animation, animationName);
        _npc.BehaviourSystem?.SwitchState(nextState);
    }

    /// <summary>
    /// Switches to the next state if the animation is finished.
    /// </summary>
    protected void SwitchStateAfterAnimation(ANPCState nextState, int animation = -1)
    {
        if (animation != -1)
            _npc.AnimationController.ChangeAnimationTo(animation);

        if (_npc.AnimationController.IsCurrentAnimationFinished())
            _npc.BehaviourSystem?.SwitchState(nextState);
    }
}
