using System;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class ATutorialStepConditionSO : ScriptableObject
{
    public event Action Triggered;
    public event Action Completed;

    public virtual void SetUIDocument(UIDocument uiDocument) { }
    public virtual void BeginListening() { }
    public virtual void EndListening() { }
    public virtual void Tick(float deltaTime) { }

    protected void MarkTriggered() => Triggered?.Invoke();
    protected void MarkCompleted() => Completed?.Invoke();
}
