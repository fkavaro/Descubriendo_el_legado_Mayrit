using System;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class AHUDState : AUIState
{
    #region PROPERTIES
    public event Action OnPanelShownEvent;
    public event Action OnPanelHiddenEvent;

    protected ContextualPanel _contextualPanel;
    #endregion

    #region CONSTRUCTOR
    public AHUDState(string name, UIDocument uiDocument)
    : base(name, uiDocument) { }
    #endregion

    #region INHERITED METHODS
    public override void StartState()
    {
        base.StartState();

        InitializeContextualPanel();

        _contextualPanel.OnHiddenEvent += HideContextualPanel;
        UIManager.Instance.ShowContextualPanelEvent += ShowContextualPanel;
        UIManager.Instance.HideContextualPanelEvent += HideContextualPanel;
    }

    public override void ExitState()
    {
        base.ExitState();

        _contextualPanel.OnHiddenEvent -= HideContextualPanel;
        UIManager.Instance.ShowContextualPanelEvent -= ShowContextualPanel;
        UIManager.Instance.HideContextualPanelEvent -= HideContextualPanel;
    }
    #endregion

    #region PROTECTED METHODS
    void InitializeContextualPanel()
    {
        VisualElement contextualPanelRoot = _screen.Q<VisualElement>("ContextualPanel");

        if (contextualPanelRoot == null)
            Debug.LogWarning("_contextualPanel not found");

        _contextualPanel = new(contextualPanelRoot);
    }

    protected void ShowContextualPanel(AInformationSO objectInfo)
    {
        _contextualPanel.ShowInfo(objectInfo);
        OnContextualPanelShown();
        OnPanelShownEvent?.Invoke();
    }
    #endregion

    #region PRIVATE METHODS
    void HideContextualPanel()
    {
        OnContextualPanelHidden();
        OnPanelHiddenEvent?.Invoke();
    }
    #endregion

    #region ABSTRACT METHODS
    protected virtual void OnContextualPanelShown() { }
    protected virtual void OnContextualPanelHidden() { }
    #endregion
}
