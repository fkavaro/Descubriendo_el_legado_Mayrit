using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class ACompositeTutorialConditionSO : ATutorialStepConditionSO
{
    [SerializeField] List<ATutorialStepConditionSO> _conditions = new();

    readonly List<ATutorialStepConditionSO> _runtimeConditions = new();
    UIDocument _uiDocument;

    protected IReadOnlyList<ATutorialStepConditionSO> RuntimeConditions => _runtimeConditions;

    public override void SetUIDocument(UIDocument uiDocument)
    {
        _uiDocument = uiDocument;
    }

    public override void BeginListening()
    {
        EndListening();

        foreach (ATutorialStepConditionSO conditionAsset in _conditions)
        {
            if (conditionAsset == null)
                continue;

            ATutorialStepConditionSO runtimeCondition = Instantiate(conditionAsset);
            runtimeCondition.SetUIDocument(_uiDocument);
            runtimeCondition.Completed += OnChildConditionCompleted;
            runtimeCondition.BeginListening();

            _runtimeConditions.Add(runtimeCondition);
        }

        if (_runtimeConditions.Count == 0)
            Debug.LogWarning($"Composite tutorial condition '{name}' has no child conditions.");

        OnBeginListeningCompleted();
    }

    public override void EndListening()
    {
        for (int i = 0; i < _runtimeConditions.Count; i++)
        {
            ATutorialStepConditionSO runtimeCondition = _runtimeConditions[i];
            if (runtimeCondition == null)
                continue;

            runtimeCondition.Completed -= OnChildConditionCompleted;
            runtimeCondition.EndListening();
            Destroy(runtimeCondition);
        }

        _runtimeConditions.Clear();
        OnEndedListening();
    }

    public override void Tick(float deltaTime)
    {
        for (int i = 0; i < _runtimeConditions.Count; i++)
            _runtimeConditions[i]?.Tick(deltaTime);
    }

    protected void CompleteComposite()
    {
        EndListening();
        MarkCompleted();
    }

    protected virtual void OnBeginListeningCompleted() { }
    protected virtual void OnEndedListening() { }
    protected abstract void OnChildConditionCompleted();
}
