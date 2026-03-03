using UnityEngine;

[CreateAssetMenu(fileName = "AllTutorialConditionSO", menuName = "Scriptable Objects/Tutorial Conditions/All (AND)")]
public class AllTutorialConditionSO : ACompositeTutorialConditionSO
{
    int _completedChildren;

    protected override void OnBeginListeningCompleted()
    {
        _completedChildren = 0;

        if (RuntimeConditions.Count == 0)
            CompleteComposite();
    }

    protected override void OnEndedListening()
    {
        _completedChildren = 0;
    }

    protected override void OnChildConditionCompleted()
    {
        _completedChildren++;

        if (_completedChildren >= RuntimeConditions.Count)
            CompleteComposite();
    }
}
