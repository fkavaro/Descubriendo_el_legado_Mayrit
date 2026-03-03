using UnityEngine;
using UnityEngine.UIElements;

public class TutorialState : AUIState
{
    VisualElement _tutorialScreen;

    public TutorialState(TutorialStepSO tutorialStepData, UIDocument uiDocument)
    : base(tutorialStepData.VisualElementName, uiDocument) { }

    protected override void ConfigureUIElementsOnAwake()
    {
        _tutorialScreen = GetByName<VisualElement>("TutorialSteps", _UIDocument.rootVisualElement);
        _tutorialScreen.style.display = DisplayStyle.None;
    }

    public override void StartState()
    {
        _tutorialScreen.style.display = DisplayStyle.Flex;
        base.StartState();
    }
}
