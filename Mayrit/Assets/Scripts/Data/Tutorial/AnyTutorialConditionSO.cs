using UnityEngine;

[CreateAssetMenu(fileName = "AnyTutorialConditionSO", menuName = "Scriptable Objects/Tutorial Conditions/Any (OR)")]
public class AnyTutorialConditionSO : ACompositeTutorialConditionSO
{
    protected override void OnChildConditionCompleted()
    {
        CompleteComposite();
    }
}
