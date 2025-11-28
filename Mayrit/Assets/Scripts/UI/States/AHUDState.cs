using System;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class AHUDState : AUIState
{
    #region PROPERTIES
    public event Action OnPlayCharacterEvent;
    public event Action ContextualPanelHiddenEvent;
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

        _contextualPanel.PlayCharacterClickedEvent += () => OnPlayCharacterEvent?.Invoke();
        _contextualPanel.HiddenEvent += () => ContextualPanelHiddenEvent?.Invoke();
        _contextualPanel.ShownEvent += OnContextualPanelShown;
        _contextualPanel.HiddenEvent += OnContextualPanelHidden;
        UIManager.Instance.ShowContextualPanelEvent += ShowContextualPanel;
        UIManager.Instance.HideContextualPanelEvent += HideContextualPanel;
    }

    public override void ExitState()
    {
        base.ExitState();

        _contextualPanel.PlayCharacterClickedEvent -= () => OnPlayCharacterEvent?.Invoke();
        _contextualPanel.HiddenEvent -= () => ContextualPanelHiddenEvent?.Invoke();
        _contextualPanel.ShownEvent -= OnContextualPanelShown;
        _contextualPanel.HiddenEvent -= OnContextualPanelHidden;
        UIManager.Instance.ShowContextualPanelEvent -= ShowContextualPanel;
        UIManager.Instance.HideContextualPanelEvent -= HideContextualPanel;
    }
    #endregion

    #region PROTECTED METHODS
    protected void ShowContextualPanel(DataSO data, bool isCharacterData = false)
    {
        _contextualPanel.ShowInfo(data, isCharacterData);
    }

    protected void HideContextualPanel()
    {
        _contextualPanel.Hide();
    }
    #endregion

    #region PRIVATE METHODS
    void InitializeContextualPanel()
    {
        VisualElement hudScreen = _UIDocument.rootVisualElement.Q<VisualElement>("HUD");
        VisualElement contextualPanelRoot = hudScreen.Q<VisualElement>("ContextualPanel");

        if (contextualPanelRoot == null)
        {
            Debug.LogWarning("_contextualPanel not found");
            return;
        }

        _contextualPanel = new(contextualPanelRoot);
    }
    #endregion

    #region VIRTUAL METHODS
    protected virtual void OnContextualPanelShown() { }
    protected virtual void OnContextualPanelHidden() { }
    #endregion
}
