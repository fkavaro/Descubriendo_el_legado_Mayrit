using UnityEngine;

public class PlaySFX : StateMachineBehaviour
{
    [SerializeField] private SFXType _enterSFXType = SFXType.None;
    [SerializeField] private SFXType _exitSFXType = SFXType.None;
    [SerializeField, Range(0, 1)] private float _volume = 1f;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        SoundManager soundManager = ServiceLocator.Instance.Get<SoundManager>();

        if (soundManager != null)
            soundManager.PlaySFX(_enterSFXType, _volume);
        else
            Debug.LogError("PlaySFXEnter: SoundManager service not found!");
    }

    //OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        SoundManager soundManager = ServiceLocator.Instance.Get<SoundManager>();

        if (soundManager != null)
            soundManager.PlaySFX(_exitSFXType, _volume);
        else
            Debug.LogError("PlaySFXEnter: SoundManager service not found!");
    }
}
