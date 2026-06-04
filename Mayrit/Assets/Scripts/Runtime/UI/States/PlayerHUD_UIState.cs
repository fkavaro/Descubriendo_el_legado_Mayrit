using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerHUD_UIState : AHUDState
{
    #region  PROPERTIES
    TourManager _tourManager;
    CollectiblesManager _collectiblesManager;

    VisualElement _onTourEndVisual,
        _tourArea,
        _stopsArea,
        _collectiblesArea,
        _hintsArea;
    Label _nextStopLabel,
        _completedTourStopsLabel,
        _totalTourStopsLabel,
        _foundCollectiblesLabel,
        _totalCollectiblesLabel;
    #endregion

    #region CONSTRUCTOR
    public PlayerHUD_UIState(UIDocument uiDocument, float fadeInDuration, float fadeOutDuration)
    : base("PlayerHUD", uiDocument, fadeInDuration, fadeOutDuration) { }
    #endregion

    #region UI STATE INHERITED METHODS
    protected override void ConfigureUIElementsOnAwake()
    {
        base.ConfigureUIElementsOnAwake();

        _tourArea = GetByName<VisualElement>("TourArea");
        _stopsArea = GetByName<VisualElement>("StopsArea", _tourArea);
        _nextStopLabel = GetByName<Label>("NextStop", _stopsArea);
        _completedTourStopsLabel = GetByName<Label>("CompletedTourStopsCount");
        _totalTourStopsLabel = GetByName<Label>("TotalTourStopsCount");
        _onTourEndVisual = GetByName<VisualElement>("OnTourEnd");

        _collectiblesArea = GetByName<VisualElement>("CollectiblesArea");
        _hintsArea = GetByName<VisualElement>("HintsArea", _collectiblesArea);
        _foundCollectiblesLabel = GetByName<Label>("FoundCollectiblesCount", _collectiblesArea);
        _totalCollectiblesLabel = GetByName<Label>("TotalCollectiblesCount", _collectiblesArea);
    }

    protected override void GetServicesDependenciesOnStart()
    {
        base.GetServicesDependenciesOnStart();

        _tourManager = ServiceLocator.Instance.Get<TourManager>();
        _collectiblesManager = ServiceLocator.Instance.Get<CollectiblesManager>();

        if (_tourManager == null)
            Debug.LogWarning("PlayerHUD_UIState: No TourManager found in ServiceLocator on StartState");
        if (_collectiblesManager == null)
            Debug.LogWarning("PlayerHUD_UIState: No CollectiblesManager found in ServiceLocator on StartState");
    }

    public override void StartState()
    {
        base.StartState();

        _tourManager.TourStopVisitedEvent += OnTourStopVisited;
        _collectiblesManager.OnCollectibleFoundEvent += OnCollectibleFound;

        ShowTourEndVisual(_tourManager.CurrentTour.IsCompleted);
        UpdateTourStopsUI();
        UpdateCollectiblesUI();
        _compass.IsNextTourStopShown = true;
    }

    public override void ExitState()
    {
        base.ExitState();

        _tourManager.TourStopVisitedEvent -= OnTourStopVisited;
        _collectiblesManager.OnCollectibleFoundEvent -= OnCollectibleFound;

        // Unlock cursor and make it visible (has been lock in 3rd person camera state start)
        UnityEngine.Cursor.lockState = CursorLockMode.None;

        _compass.IsNextTourStopShown = false;
    }
    #endregion

    #region HUD STATE INHERITED METHODS
    protected override void OnContextualPanelShown()
    {
        _tourArea.style.display = DisplayStyle.None;
        _onTourEndVisual.style.display = DisplayStyle.None;
    }

    protected override void OnContextualPanelHidden()
    {
        _tourArea.style.display = DisplayStyle.Flex;
        ShowTourEndVisual(_tourManager.CurrentTour.IsCompleted);
    }
    #endregion

    #region PRIVATE METHODS
    void ShowTourEndVisual(bool show)
    {
        if (show)
        {
            _stopsArea.style.display = DisplayStyle.None;
            _onTourEndVisual.style.display = DisplayStyle.Flex;
        }
        else
        {
            _stopsArea.style.display = DisplayStyle.Flex;
            _onTourEndVisual.style.display = DisplayStyle.None;
        }
    }

    void UpdateTourStopsUI()
    {
        _nextStopLabel.text = _tourManager.NextTourStop != null ? $"{_tourManager.NextTourStop.Data.Header}" : "Tour completado";
        _completedTourStopsLabel.text = $"{_tourManager.CurrentTour.ReachedCount}";
        _totalTourStopsLabel.text = $"{_tourManager.CurrentTour.TotalCount}";
    }

    void UpdateCollectiblesUI()
    {
        int foundCollectibles = _collectiblesManager.FoundCollectiblesCount;
        int totalCollectibles = _collectiblesManager.TotalCollectiblesCount;

        _foundCollectiblesLabel.text = $"{foundCollectibles}";
        _totalCollectiblesLabel.text = $"{totalCollectibles}";

        if (foundCollectibles >= totalCollectibles)
            _hintsArea.style.display = DisplayStyle.None;
        else
            _hintsArea.style.display = DisplayStyle.Flex;
    }

    void OnTourStopVisited(TourStop tourStop)
    {
        UpdateTourStopsUI();
    }

    void OnCollectibleFound(Collectible collectible)
    {
        UpdateCollectiblesUI();
    }
    #endregion

    /*
        void PopulateTourStopsUI()
        {
            if (_currentTour == null || _stopsArea == null) return;

            _stopsArea.Clear();
            _tourStopLabels.Clear();

            TourStop[] tourStops = _currentTour.GetComponentsInChildren<TourStop>();
            bool hasSetNextStop = false;
            foreach (TourStop stop in tourStops)
            {
                if (stop.Data == null) continue;

                Label label = new(stop.Data.Header);
                label.AddToClassList("HUDText");
                if (stop == _currentTour.NextTourStop)
                {
                    label.AddToClassList("highlighted");
                    hasSetNextStop = true;
                }
                else if (!hasSetNextStop && !stop.IsVisited)
                {
                    label.AddToClassList("highlighted");
                    hasSetNextStop = true;
                }
                else if (stop.IsVisited)
                    label.AddToClassList("disabled");

                label.name = $"TourStop_{stop.GetInstanceID()}";
                _stopsArea.Add(label);
                _tourStopLabels[stop] = label;
            }
        }

        void ClearTourStopsUI()
        {
            if (_stopsArea == null) return;
            _stopsArea.Clear();
            _tourStopLabels.Clear();
        }
    */
}