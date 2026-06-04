using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class TourManager : MonoBehaviour
{
    #region PROPERTY HELPERS
    public Tour CurrentTour => _currentTour;
    public TourStop NextTourStop => _currentTour != null ? _currentTour.NextStop : null;
    #endregion

    #region EDITOR PROPERTIES
    [SerializeField] Tour _currentTour;
    #endregion

    #region INTERNAL PROPERTIES
    public event Action<TourStop> TourStopVisitedEvent;
    public event Action<TourStop> NextTourStopChangeEvent;
    public event Action<Tour> TourCompletedEvent;

    // Dependency Injection
    ScenesController _scenesController;
    UIManager _uiManager;
    SoundManager _soundManager;
    PlayableCharacter _playableCharacter;
    ProgressManager _progressManager;
    #endregion

    #region LIFE CYCLE
    void Awake()
    {
        ServiceLocator.Instance.Register(this);
    }

    void Start()
    {
        // Get dependencies from Service Locator
        _scenesController = ServiceLocator.Instance.Get<ScenesController>();
        _uiManager = ServiceLocator.Instance.Get<UIManager>();
        _soundManager = ServiceLocator.Instance.Get<SoundManager>();
        _progressManager = ServiceLocator.Instance.Get<ProgressManager>();

        // Subscribe to events
        _scenesController.SceneLoadedPartiallyEvent += OnSceneLoadedPartially;
        _uiManager.PlayTourClickedEvent += OnPlayTourClicked;
        _uiManager.ResetTourClickedEvent += OnResetTourClicked;

        AttachToTour(ServiceLocator.Instance.Get<Tour>());
    }

    void OnDisable()
    {

        // Unsubscribe from events
        _scenesController.SceneLoadedPartiallyEvent -= OnSceneLoadedPartially;

        _uiManager.PlayTourClickedEvent -= OnPlayTourClicked;
        _uiManager.ResetTourClickedEvent -= OnResetTourClicked;

        DetachFromCurrentTour();

        ServiceLocator.Instance.Unregister(this);
    }
    #endregion

    #region PRIVATE METHODS
    void AttachToTour(Tour tour)
    {
        if (tour == null) return;
        if (_currentTour == tour) return;

        // Detach previous
        DetachFromCurrentTour();

        // Update current
        _currentTour = tour;
        _currentTour.OnVisitedTourStopEvent += OnTourStopVisited;
        _currentTour.OnNextTourStopChangeEvent += OnNextTourStopChange;
        _currentTour.OnTourCompletedEvent += OnTourCompleted;
    }

    void DetachFromCurrentTour()
    {
        if (_currentTour == null) return;

        _currentTour.OnVisitedTourStopEvent -= OnTourStopVisited;
        _currentTour.OnNextTourStopChangeEvent -= OnNextTourStopChange;
        _currentTour.OnTourCompletedEvent -= OnTourCompleted;
    }
    #endregion

    #region CALLBACK METHODS
    void OnSceneLoadedPartially(SceneDatabase.SceneType type, SceneDatabase.SceneName name)
    {
        // Milestone loaded: attach to its tour
        if (type == SceneDatabase.SceneType.Milestone)
        {
            AttachToTour(ServiceLocator.Instance.Get<Tour>());
            _uiManager.ContextualPanelHiddenEvent += OnContextualPanelHidden;
            _playableCharacter = ServiceLocator.Instance.Get<PlayableCharacter>();

            if (_playableCharacter == null)
            {
                Debug.LogWarning($"[TourManager] PlayableCharacter not found in ServiceLocator.");
                return;
            }

            if (_progressManager.WasCurrentMilestoneCompleted)
            {
                _currentTour.MarkAsCompleted();
                _playableCharacter.LocateAt(_currentTour.LastStopInList.transform);
            }
        }
    }

    void OnTourStopVisited(TourStop tourStop) => TourStopVisitedEvent?.Invoke(tourStop);

    void OnNextTourStopChange(TourStop tourStop) => NextTourStopChangeEvent?.Invoke(tourStop);

    void OnContextualPanelHidden()
    {
        // Invoke completed event if tour is completed
        if (_currentTour != null && _currentTour.IsCompleted)
        {
            _soundManager.PlayTourEndSFX();
            _uiManager.ContextualPanelHiddenEvent -= OnContextualPanelHidden;
        }
    }

    void OnTourCompleted()
    {
        TourCompletedEvent?.Invoke(_currentTour);
    }

    void OnPlayTourClicked()
    {
        if (_currentTour != null)
        {
            _currentTour.StartTour();
            _soundManager.PlayTourStartSFX();
        }
        else
        {
            Debug.LogWarning($"[TourManager] No current tour to start when PlayTourClicked.");
        }
    }

    void OnResetTourClicked()
    {
        _currentTour.Reset();
        OnPlayTourClicked();
    }
    #endregion
}

