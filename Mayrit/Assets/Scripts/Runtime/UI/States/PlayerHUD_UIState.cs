using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerHUD_UIState : AHUDState
{
    #region  PROPERTIES
    TourManager _tourManager;
    CollectiblesManager _collectiblesManager;

    VisualElement _tourCompletedVisual,
        _collectionCompletedVisual,
        _tourArea,
        _tourCompletedArea,
        _stopsArea,
        _collectiblesArea,
        _collectionCompletedArea,
        _hintsArea,
        _hintsList;
    Label _nextStopLabel,
        _completedTourStopsLabel,
        _totalTourStopsLabel,
        _foundCollectiblesLabel,
        _totalCollectiblesLabel;

    bool _shownCompletedTourVisual,
        _shownCompletedCollectionVisual;
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
        _tourCompletedArea = GetByName<VisualElement>("TourCompletedArea", _tourArea);
        _stopsArea = GetByName<VisualElement>("StopsArea", _tourArea);
        _nextStopLabel = GetByName<Label>("NextStop", _stopsArea);
        _completedTourStopsLabel = GetByName<Label>("CompletedTourStopsCount");
        _totalTourStopsLabel = GetByName<Label>("TotalTourStopsCount");
        _tourCompletedVisual = GetByName<VisualElement>("TourCompletedVisual");

        _collectiblesArea = GetByName<VisualElement>("CollectiblesArea");

        _hintsArea = GetByName<VisualElement>("HintsArea", _collectiblesArea);
        _collectionCompletedArea = GetByName<VisualElement>("CollectionCompletedArea", _collectiblesArea);
        _hintsList = GetByName<VisualElement>("HintsList", _hintsArea);
        _foundCollectiblesLabel = GetByName<Label>("FoundCollectiblesCount", _collectiblesArea);
        _totalCollectiblesLabel = GetByName<Label>("TotalCollectiblesCount", _collectiblesArea);
        _collectionCompletedVisual = GetByName<VisualElement>("CollectionCompletedVisual");
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

        _progressManager.MilestoneChangedEvent += OnMilestoneChanged;

        _shownCompletedTourVisual = false;
        _shownCompletedCollectionVisual = false;
    }

    public override void StartState()
    {
        base.StartState();

        if (!_tourManager.CurrentTour.IsCompleted)
            _shownCompletedTourVisual = false;
        if (!_collectiblesManager.CurrentTracker.IsCompleted)
            _shownCompletedCollectionVisual = false;

        UpdateTourStopsUI();
        UpdateCollectiblesUI();
        _compass.IsNextTourStopShown = true;
    }

    public override void ExitState()
    {
        base.ExitState();

        // Unlock cursor and make it visible (has been lock in 3rd person camera state start)
        UnityEngine.Cursor.lockState = CursorLockMode.None;

        _compass.IsNextTourStopShown = false;
    }
    #endregion

    #region HUD STATE INHERITED METHODS
    protected override void OnContextualPanelShown()
    {
        _tourArea.style.display = DisplayStyle.None;
        _collectiblesArea.style.display = DisplayStyle.None;
        _tourCompletedVisual.style.display = DisplayStyle.None;
        _collectionCompletedVisual.style.display = DisplayStyle.None;
    }

    protected override void OnContextualPanelHidden()
    {
        _tourArea.style.display = DisplayStyle.Flex;
        _collectiblesArea.style.display = DisplayStyle.Flex;
        UpdateTourStopsUI();
        UpdateCollectiblesUI();
    }
    #endregion

    #region PRIVATE METHODS
    void UpdateTourStopsUI()
    {
        _nextStopLabel.text = _tourManager.CurrentTourStop != null ? $"{_tourManager.CurrentTourStop.Data.Header}" : "Tour completado";
        _completedTourStopsLabel.text = $"{_tourManager.CurrentTour.ReachedCount}";
        _totalTourStopsLabel.text = $"{_tourManager.CurrentTour.TotalCount}";

        if (_tourManager.CurrentTour.IsCompleted)
        {
            _stopsArea.style.display = DisplayStyle.None;
            _tourCompletedArea.style.display = DisplayStyle.Flex;

            if (!_shownCompletedTourVisual)
            {
                _tourCompletedVisual.style.display = DisplayStyle.Flex;
                _shownCompletedTourVisual = true;
            }
            else
                _tourCompletedVisual.style.display = DisplayStyle.None;
        }
        else
        {
            _stopsArea.style.display = DisplayStyle.Flex;
            _tourCompletedArea.style.display = DisplayStyle.None;
            _tourCompletedVisual.style.display = DisplayStyle.None;
        }
    }

    void UpdateCollectiblesUI()
    {
        int foundCollectibles = _collectiblesManager.FoundCollectiblesCount;
        int totalCollectibles = _collectiblesManager.TotalCollectiblesCount;

        _foundCollectiblesLabel.text = $"{foundCollectibles}";
        _totalCollectiblesLabel.text = $"{totalCollectibles}";

        if (_collectiblesManager.CurrentTracker.IsCompleted)
        {
            _hintsArea.style.display = DisplayStyle.None;
            _collectionCompletedArea.style.display = DisplayStyle.Flex;

            if (!_shownCompletedCollectionVisual)
            {
                _collectionCompletedVisual.style.display = DisplayStyle.Flex;
                _shownCompletedCollectionVisual = true;
            }
            else
                _collectionCompletedVisual.style.display = DisplayStyle.None;
        }
        else
        {
            _hintsArea.style.display = DisplayStyle.Flex;
            PopulateCollectiblesHints();
            _collectionCompletedArea.style.display = DisplayStyle.None;
            _collectionCompletedVisual.style.display = DisplayStyle.None;
        }
    }

    void PopulateCollectiblesHints()
    {
        _hintsList.Clear();

        foreach (string hint in _collectiblesManager.CurrentCollectible.Data.Hints)
        {
            Label hintLabel = new(hint);
            hintLabel.AddToClassList("HUDtext");
            hintLabel.name = $"Hint";
            _hintsList.Add(hintLabel);
        }
    }

    void Reset()
    {
        _shownCompletedTourVisual = false;
        _shownCompletedCollectionVisual = false;
    }
    #endregion

    #region CALLBACK METHODS
    void OnMilestoneChanged(Milestone_DataSO sO)
    {
        Reset();
    }
    #endregion
}