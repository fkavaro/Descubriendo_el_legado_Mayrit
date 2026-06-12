using System;
using UnityEngine;
using UnityEngine.UIElements;
public class CreditsScreen_UIState : AUIState
{
    public Action CreditsClosedEvent;

    public CreditsScreen_UIState(UISystem uiSystem, UIDocument uiDocument, float fadeInDuration, float fadeOutDuration)
    : base(uiSystem, "CreditsScreen", uiDocument, fadeInDuration, fadeOutDuration) { }

    protected override void ConfigureUIElementsOnAwake()
    {
        Button closeButton = GetButtonAndRegisterCallback("CloseButton", OnCloseClicked);
    }

    void OnCloseClicked(ClickEvent evt)
    {
        base.ExitState();
        _soundSystem.PlayButtonClickSFX();
        CreditsClosedEvent?.Invoke();
    }
}