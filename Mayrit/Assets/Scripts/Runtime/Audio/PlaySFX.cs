using UnityEngine;

public class PlaySFX : StateMachineBehaviour
{
    [SerializeField] private SoundDatabase.SFXType _enterSFXType = SoundDatabase.SFXType.None;
    [SerializeField] private SoundDatabase.SFXType _exitSFXType = SoundDatabase.SFXType.None;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        SoundManager soundManager = ServiceLocator.Instance.Get<SoundManager>();

        if (soundManager != null)
            soundManager.PlaySFX(_enterSFXType);
        else
            Debug.LogError("PlaySFXEnter: SoundManager service not found!");
    }

    //OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        SoundManager soundManager = ServiceLocator.Instance.Get<SoundManager>();

        if (soundManager != null)
            soundManager.PlaySFX(_exitSFXType);
        else
            Debug.LogError("PlaySFXEnter: SoundManager service not found!");
    }
}
