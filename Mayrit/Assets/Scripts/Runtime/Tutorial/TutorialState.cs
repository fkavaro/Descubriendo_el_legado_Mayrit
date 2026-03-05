using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TutorialState : AUIState
{
    public TutorialStepSO Data => _data;
    readonly TutorialStepSO _data;
    readonly AStateMachine<TutorialState> _fsm;
    readonly ATutorialStepConditionSO _completionCondition;
    readonly List<VisualElement> _hiddenElements = new();
    bool _animationStarted;

    public TutorialState(TutorialStepSO tutorialStepData, UIManager uiManager, AStateMachine<TutorialState> fsm)
    : base(tutorialStepData.VisualElementName, uiManager.UIDocument)
    {
        _data = tutorialStepData;
        _completionCondition = _data.CompletionCondition;

        _completionCondition.SetUIDocument(_UIDocument);

        if (_completionCondition is ContextualPanelShownConditionSO selectionCondition)
            selectionCondition.SetUIManager(uiManager);

        _fsm = fsm;
    }

    protected override void ConfigureUIElementsOnAwake()
    {
        foreach (UIElementsToHide elementName in _data.VisualElementsToHide)
        {
            VisualElement elementToHide = GetByName<VisualElement>(elementName.ToString(), _UIDocument.rootVisualElement);

            if (elementToHide != null)
                _hiddenElements.Add(elementToHide);
        }
    }

    public override void StartState()
    {
        _animationStarted = false;
        _completionCondition.Triggered += OnConditionTriggered;
        _completionCondition.Completed += OnCompletionConditionCompleted;
        _completionCondition.BeginListening();

        foreach (VisualElement element in _hiddenElements)
            element.style.display = DisplayStyle.None;

        base.StartState();
    }

    public override void UpdateState()
    {
        base.UpdateState();

        _completionCondition.Tick(Time.deltaTime);
    }

    void OnConditionTriggered()
    {
        _completionCondition.Triggered -= OnConditionTriggered;
        _animationStarted = true;

        if (_data.AnimationDuration > 0f)
            _uiManager.StartCoroutine(ScalePunchAnimation(_screen, _data.AnimationPeakScale, _data.AnimationDuration));
    }

    void OnCompletionConditionCompleted()
    {
        _completionCondition.Triggered -= OnConditionTriggered;
        _completionCondition.Completed -= OnCompletionConditionCompleted;
        _completionCondition.EndListening();

        if (!_animationStarted && _data.AnimationDuration > 0f)
            _uiManager.StartCoroutine(ScalePunchAnimation(_screen, _data.AnimationPeakScale, _data.AnimationDuration, OnAnimationComplete));
        else
            OnAnimationComplete();
    }

    void OnAnimationComplete()
    {
        foreach (VisualElement element in _hiddenElements)
            element.style.display = DisplayStyle.Flex;

        _hiddenElements.Clear();

        _fsm.SwitchToNextStateInSequence(out _);
    }
}
