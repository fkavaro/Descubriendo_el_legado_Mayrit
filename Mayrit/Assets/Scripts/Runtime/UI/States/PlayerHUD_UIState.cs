using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerHUD_UIState : AHUDState
{
    #region  PROPERTIES
    TourManager TourManager => _gameManager.TourManager;
    CollectiblesManager CollectiblesManager => _gameManager.CollectiblesManager;

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

    public bool ShownCompletedTourVisual,
        ShownCompletedCollectionVisual;
    #endregion

    #region CONSTRUCTOR
    public PlayerHUD_UIState(UISystem uiSystem, UIDocument uiDocument, float fadeInDuration, float fadeOutDuration)
    : base(uiSystem, "PlayerHUD", uiDocument, fadeInDuration, fadeOutDuration) { }
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

        Reset();
    }

    public override void StartState()
    {
        base.StartState();

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

    #region PUBLIC METHODS
    public void Reset()
    {
        ShownCompletedTourVisual = false;
        ShownCompletedCollectionVisual = false;
    }
    #endregion

    #region PRIVATE METHODS
    void UpdateTourStopsUI()
    {
        _nextStopLabel.text = TourManager.CurrentTourStop != null ? $"{TourManager.CurrentTourStop.Data.Header}" : "Tour completado";
        _completedTourStopsLabel.text = $"{TourManager.CurrentTour.ReachedCount}";
        _totalTourStopsLabel.text = $"{TourManager.CurrentTour.TotalCount}";

        if (TourManager.CurrentTour.IsCompleted)
        {
            _stopsArea.style.display = DisplayStyle.None;
            _tourCompletedArea.style.display = DisplayStyle.Flex;

            if (!ShownCompletedTourVisual)
            {
                _tourCompletedVisual.style.display = DisplayStyle.Flex;
                ShownCompletedTourVisual = true;
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
        int foundCollectibles = CollectiblesManager.FoundCollectiblesCount;
        int totalCollectibles = CollectiblesManager.TotalCollectiblesCount;

        _foundCollectiblesLabel.text = $"{foundCollectibles}";
        _totalCollectiblesLabel.text = $"{totalCollectibles}";

        if (CollectiblesManager.CurrentTracker.IsCompleted)
        {
            _hintsArea.style.display = DisplayStyle.None;
            _collectionCompletedArea.style.display = DisplayStyle.Flex;

            if (!ShownCompletedCollectionVisual)
            {
                _collectionCompletedVisual.style.display = DisplayStyle.Flex;
                ShownCompletedCollectionVisual = true;
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

        foreach (string hint in CollectiblesManager.CurrentCollectible.Data.Hints)
        {
            Label hintLabel = new(hint);
            hintLabel.AddToClassList("HUDtext");
            hintLabel.name = $"Hint";
            _hintsList.Add(hintLabel);
        }
    }
    #endregion
}