using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class TourManager : MonoBehaviour
{
    #region PROPERTY HELPERS
    public Tour CurrentTour => _currentTour;
    #endregion

    #region EDITOR PROPERTIES
    [Header("Tour manager")]
    [SerializeField] Tour _currentTour;
    [SerializeField] TourStop _nextTourStop;

    /* TODO remove
    [Header("Path visualizer settings")]
    [Tooltip("Meters between samples along segments. Lower = smoother")]
    [SerializeField] float _pointSpacing = 0.5f;
    [Tooltip("Max distance to snap player/TourStop to NavMesh")]
    [SerializeField] float _endpointSnapDistance = 2f;
    [Tooltip("Max distance to project points down to NavMesh terrain")]
    [SerializeField] float _terrainProjectionDistance = 1f;
    [Tooltip("Vertical offset to lift line above ground (prevents z-fighting)")]
    [SerializeField] float _heightOffset = 0.03f;
    [Tooltip("Hard limit on total points for performance")]
    [SerializeField] int _maxPointCount = 2000;
    [Tooltip("Maximum distance from start to render (trail cutoff)")]
    [SerializeField] int _maxTrailLength = 100;
    */
    #endregion

    #region INTERNAL PROPERTIES
    public event Action<TourStop> TourStopVisitedEvent;
    public event Action<TourStop> NextTourStopChangeEvent;
    public event Action<Tour> TourCompletedEvent;

    // TODO remove
    // PathVisualizer _pathVisualizer; 

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

        // TODO remove
        // _pathVisualizer = new PathVisualizer(
        //     GetComponent<LineRenderer>(),
        //     _pointSpacing,
        //     _endpointSnapDistance,
        //     _terrainProjectionDistance,
        //     _heightOffset,
        //     _maxPointCount,
        //     _maxTrailLength);
        // _pathVisualizer.Initialize();

        AttachToTour(ServiceLocator.Instance.Get<Tour>());
    }

    // TODO remove
    // void Update()
    // {
    //     if (_playableCharacter != null && _playableCharacter.IsBeingControlled &&
    //         _currentTour != null && !_currentTour.IsCompleted)
    //         _pathVisualizer.UpdatePath(_playableCharacter.transform);
    //     else
    //         _pathVisualizer.Clear();
    // }

    void OnDisable()
    {
        // TODO remove
        // // Let the visualizer unsubscribe from ProgressManager and cleanup
        //_pathVisualizer.Deinitialize();

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
        _nextTourStop = null;
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
                _playableCharacter.LocateAt(_currentTour.LastTourStopinList.transform);
            }
        }
    }

    void OnTourStopVisited(TourStop tourStop)
    {
        TourStopVisitedEvent?.Invoke(tourStop);
    }

    void OnNextTourStopChange(TourStop tourStop)
    {
        NextTourStopChangeEvent?.Invoke(tourStop);
        _nextTourStop = tourStop;
    }

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

