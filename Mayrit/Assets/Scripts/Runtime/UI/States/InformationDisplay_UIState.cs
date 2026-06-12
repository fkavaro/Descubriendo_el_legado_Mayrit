using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class InformationDisplay_UIState : AUIState
{
    #region PROPERTIES
    public event Action PlayTourClickedEvent;
    public event Action ResetTourClickedEvent;
    public event Action ClosedEvent;
    public event Action PauseClickedEvent;

    public DataSO DataToShow;

    readonly ContextualPanelComponent _contextualPanelComponent;

    Button _pauseButton;
    #endregion


    public InformationDisplay_UIState(UISystem uiSystem, UIDocument uiDocument, float fadeInDuration, float fadeOutDuration, ContextualPanelComponent contextualPanelComponent)
    : base(uiSystem, "InformationDisplay", uiDocument, fadeInDuration, fadeOutDuration)
    {
        _contextualPanelComponent = contextualPanelComponent;
    }

    protected override void ConfigureUIElementsOnAwake()
    {
        _pauseButton = GetButtonAndRegisterCallback("PauseButton", OnPauseClicked);
    }

    public override void StartState()
    {
        base.StartState();

        _contextualPanelComponent.ContinueClickedEvent += OnStartTour;
        _contextualPanelComponent.ResetTourClickedEvent += OnResetTour;
        _contextualPanelComponent.ClosedEvent += OnCloseButton;

        if (DataToShow == null)
        {
            Debug.LogError("InformationDisplay_UIState: DataToShow is null!");
            return;
        }

        _contextualPanelComponent.ShowData(DataToShow);

    }

    public override void ExitState()
    {
        base.ExitState();

        _contextualPanelComponent.ExitState();

        _contextualPanelComponent.ContinueClickedEvent -= OnStartTour;
        _contextualPanelComponent.ResetTourClickedEvent -= OnResetTour;
        _contextualPanelComponent.ClosedEvent -= OnCloseButton;
    }

    void OnPauseClicked(ClickEvent evt)
    {
        _soundSystem.PlayButtonClickSFX();
        PauseClickedEvent?.Invoke();
    }

    void OnCloseButton()
    {
        ClosedEvent?.Invoke();
    }

    void OnStartTour()
    {
        _soundSystem.PlayTourStartSFX();
        PlayTourClickedEvent?.Invoke();
    }

    void OnResetTour()
    {
        _soundSystem.PlayTourStartSFX();
        ResetTourClickedEvent?.Invoke();
    }
}
