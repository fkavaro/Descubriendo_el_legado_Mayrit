using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerHUD_UIState : AHUDState
{
    #region  PROPERTIES
    public bool _showTourEnd;
    Tour _currentTour;
    TourManager _tourManager;

    Button _pauseButton;
    VisualElement _tourArea,
        _onTourEndVisual,
        _tourStopsList;
    Label _tourName,
        _tourDescription;
    readonly Dictionary<TourStop, Label> _tourStopLabels = new();
    #endregion

    #region CONSTRUCTOR
    public PlayerHUD_UIState(UIDocument uiDocument, float fadeInDuration, float fadeOutDuration)
    : base("PlayerHUD", uiDocument, fadeInDuration, fadeOutDuration) { }
    #endregion

    #region UI STATE INHERITED METHODS
    protected override void ConfigureUIElementsOnAwake()
    {
        base.ConfigureUIElementsOnAwake();

        _pauseButton = GetButtonAndRegisterCallback("PauseButton", OnPauseClicked);
        _tourArea = GetByName<VisualElement>("TourArea");
        _tourName = GetByName<Label>("Name", _tourArea);
        _tourDescription = GetByName<Label>("Description", _tourArea);
        _tourStopsList = GetByName<VisualElement>("TourStopsList");
        _onTourEndVisual = GetByName<VisualElement>("OnTourEnd");
    }

    protected override void GetServicesDependenciesOnStart()
    {
        base.GetServicesDependenciesOnStart();

        _currentTour = ServiceLocator.Instance.Get<Tour>();
        _tourManager = ServiceLocator.Instance.Get<TourManager>();

        if (_currentTour == null)
            Debug.LogWarning("PlayerHUD_UIState: No Tour found in ServiceLocator on StartState");
        if (_tourManager == null)
            Debug.LogWarning("PlayerHUD_UIState: No TourManager found in ServiceLocator on StartState");
    }

    public override void StartState()
    {
        base.StartState();

        PopulateTourStopsUI();
        SubscribeToTourEvents();
        ShowTourEndVisual(_showTourEnd);
        _compass.IsNextTourStopShown = true;
    }

    public override void ExitState()
    {
        base.ExitState();

        UnsubscribeFromTourEvents();
        ClearTourStopsUI();

        // Unlock cursor and make it visible (has been lock in 3rd person camera state start)
        UnityEngine.Cursor.lockState = CursorLockMode.None;

        _compass.IsNextTourStopShown = false;
    }
    #endregion

    #region HUD STATE INHERITED METHODS
    protected override void OnContextualPanelShown()
    {
        _tourArea.style.display = DisplayStyle.None;
        _tourStopsList.style.display = DisplayStyle.None;
        _onTourEndVisual.style.display = DisplayStyle.None;
    }

    protected override void OnContextualPanelHidden()
    {
        ShowTourEndVisual(_currentTour.IsCompleted);
    }
    #endregion

    #region PUBLIC METHODS
    void ShowTourEndVisual(bool show)
    {
        if (show)
        {
            _tourArea.style.display = DisplayStyle.None;
            _tourStopsList.style.display = DisplayStyle.None;
            _onTourEndVisual.style.display = DisplayStyle.Flex;
        }
        else
        {
            _onTourEndVisual.style.display = DisplayStyle.None;
            _tourArea.style.display = DisplayStyle.Flex;
            _tourStopsList.style.display = DisplayStyle.Flex;
            _tourName.text = _currentTour.Data.Header;
            _tourDescription.text = _currentTour.Data.SubHeader;
        }
    }

    void PopulateTourStopsUI()
    {
        if (_currentTour == null || _tourStopsList == null) return;

        _tourStopsList.Clear();
        _tourStopLabels.Clear();

        TourStop[] tourStops = _currentTour.GetComponentsInChildren<TourStop>();
        bool hasSetNextStop = false;
        foreach (TourStop stop in tourStops)
        {
            if (stop.Data == null) continue;

            Label label = new(stop.Data.Header);
            label.AddToClassList("tourStopItem");
            if (stop == _currentTour.NextTourStop)
            {
                label.AddToClassList("next");
                hasSetNextStop = true;
            }
            else if (!hasSetNextStop && !stop.IsVisited)
            {
                label.AddToClassList("next");
                hasSetNextStop = true;
            }
            else
                label.AddToClassList(stop.IsVisited ? "visited" : "unvisited");

            label.name = $"TourStop_{stop.GetInstanceID()}";
            _tourStopsList.Add(label);
            _tourStopLabels[stop] = label;
        }
    }

    void ClearTourStopsUI()
    {
        if (_tourStopsList == null) return;
        _tourStopsList.Clear();
        _tourStopLabels.Clear();
    }

    void SubscribeToTourEvents()
    {
        if (_tourManager != null)
            _tourManager.TourStopVisitedEvent += OnTourStopVisited;
    }

    void UnsubscribeFromTourEvents()
    {
        if (_tourManager != null)
            _tourManager.TourStopVisitedEvent -= OnTourStopVisited;
    }

    void OnTourStopVisited(TourStop tourStop)
    {
        if (tourStop.Data == null) return;

        if (_tourStopLabels.TryGetValue(tourStop, out Label label))
        {
            label.RemoveFromClassList("next");
            label.RemoveFromClassList("unvisited");
            label.AddToClassList("visited");

            // Highlight next stop
            if (_currentTour.NextTourStop != null && _tourStopLabels.TryGetValue(_currentTour.NextTourStop, out Label nextLabel))
            {
                nextLabel.RemoveFromClassList("unvisited");
                nextLabel.AddToClassList("next");
            }
        }
    }
    #endregion
}