using UnityEngine;

/// <summary>
/// Abstract class that determines the humanoid properties of the NPCs and its animations.
/// </summary>
public abstract class AHumanoid<TController> : AAnimationController<TController>
where TController : ABehaviourController<TController>
{
    #region HUMANOID ANIMATIONS
    readonly public int _animTalk = Animator.StringToHash("Talk"),
        _animPickUp = Animator.StringToHash("PickUp"),
        _animComplain = Animator.StringToHash("Complain"),
        _animStunned = Animator.StringToHash("Stunned"),
        _animSitDown = Animator.StringToHash("SitDown"),
        _animStandUp = Animator.StringToHash("StandUp"),
        _animArgue = Animator.StringToHash("Argue"),
        _animYell = Animator.StringToHash("Yell"),
        _animWait = Animator.StringToHash("Wait"),
        _animDisbelief = Animator.StringToHash("Disbelief");
    #endregion
}
