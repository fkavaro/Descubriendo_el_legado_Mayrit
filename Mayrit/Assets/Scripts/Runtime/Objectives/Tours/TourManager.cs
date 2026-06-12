using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class TourManager : MonoBehaviour
{
    #region PROPERTY HELPERS
    public Tour CurrentTour => _currentTour;
    public TourStop CurrentTourStop => _currentTour != null ? _currentTour.CurrentValidObjective : null;
    #endregion

    #region EDITOR PROPERTIES
    [SerializeField] Tour _currentTour;
    #endregion

    #region INTERNAL PROPERTIES
    public event Action<TourStop> TourStopVisitedEvent;
    public event Action<Tour> TourCompletedEvent;

    // Dependency Injection
    ScenesController _scenesController;
    GameManager _gameManager;
    SoundSystem _soundManager;
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
        _gameManager = ServiceLocator.Instance.Get<GameManager>();
        _soundManager = ServiceLocator.Instance.Get<SoundSystem>();
        _progressManager = ServiceLocator.Instance.Get<ProgressManager>();

        // Subscribe to events
        _scenesController.SceneLoadedPartiallyEvent += OnSceneLoadedPartially;
        _gameManager.TourAndPlayerResetEvent += OnResetTourClicked;

        AttachToTour(ServiceLocator.Instance.Get<Tour>());
    }

    void OnDisable()
    {

        // Unsubscribe from events
        _scenesController.SceneLoadedPartiallyEvent -= OnSceneLoadedPartially;

        _gameManager.TourAndPlayerResetEvent -= OnResetTourClicked;

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
        _currentTour.OnObjectiveReachedEvent += OnTourStopVisited;
        _currentTour.OnCompletedEvent += OnTourCompleted;
    }

    void DetachFromCurrentTour()
    {
        if (_currentTour == null) return;

        _currentTour.OnObjectiveReachedEvent -= OnTourStopVisited;
        _currentTour.OnCompletedEvent -= OnTourCompleted;
    }
    #endregion

    #region CALLBACK METHODS
    void OnSceneLoadedPartially(SceneDatabase.SceneType type, SceneDatabase.SceneName name)
    {
        // Milestone loaded: attach to its tour
        if (type == SceneDatabase.SceneType.Milestone)
        {
            AttachToTour(ServiceLocator.Instance.Get<Tour>());

            _playableCharacter = ServiceLocator.Instance.Get<PlayableCharacter>();

            if (_playableCharacter == null)
            {
                Debug.LogWarning($"[TourManager] PlayableCharacter not found in ServiceLocator.");
                return;
            }

            if (_progressManager.WasCurrentMilestoneCompleted)
            {
                _currentTour.Complete();
                _playableCharacter.LocateAt(_currentTour.LastStopInList.transform);
            }
        }
    }

    void OnTourStopVisited(TourStop tourStop) => TourStopVisitedEvent?.Invoke(tourStop);

    void OnGameStateChanged()
    {
        if (_currentTour.IsCompleted && _gameManager.IsInThirdPersonState)
        {
            _soundManager.PlayTourEndSFX();
            _gameManager.StateChangedEvent -= OnGameStateChanged;
        }
    }

    void OnTourCompleted()
    {
        TourCompletedEvent?.Invoke(_currentTour);

        _gameManager.StateChangedEvent += OnGameStateChanged;
    }

    void OnResetTourClicked()
    {
        _currentTour.Reset();
    }
    #endregion
}

