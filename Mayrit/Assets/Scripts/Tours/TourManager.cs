using System;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TourManager : MonoBehaviour
{
    #region PROPERTY HELPERS
    public Tour CurrentTour => _currentTour;
    #endregion

    #region EDITOR PROPERTIES
    [Header("Tour manager")]
    [SerializeField] Tour _currentTour;
    [SerializeField] PointOfInterest _nextPOI;

    [Header("Path visualizer settings")]
    [Tooltip("Meters between samples along segments")]
    [SerializeField] float _sampleSpacing = 0.5f;
    [Tooltip("Max distance to snap start/end to NavMesh")]
    [SerializeField] float _sampleDistance = 2f;
    [Tooltip("Max distance to project samples to NavMesh")]
    [SerializeField] float _projSampleDistance = 1f;
    [Tooltip("How much to lift the rendered line above navmesh")]
    [SerializeField] float _renderYOffset = 0.03f;
    [Tooltip("Safety cap for points to render")]
    [SerializeField] int _maxPoints = 2000;
    #endregion

    #region INTERNAL PROPERTIES
    public event Action<PointOfInterest> TourPOIVisitedEvent;
    public event Action<PointOfInterest> OnTourNextPOIChangeEvent;
    public event Action<Tour> OnTourCompletedEvent;

    PathVisualizer _pathVisualizer;

    // Dependency Injection
    ProgressManager _progressManager;
    UIManager _uiManager;
    GameManager _gameManager;
    SoundManager _soundManager;
    #endregion

    #region LIFE CYCLE
    void Start()
    {
        // Get dependencies from Service Locator
        _progressManager = ServiceLocator.Instance.Get<ProgressManager>();
        _uiManager = ServiceLocator.Instance.Get<UIManager>();
        _gameManager = ServiceLocator.Instance.Get<GameManager>();
        _soundManager = ServiceLocator.Instance.Get<SoundManager>();

        // Subscribe to events
        _progressManager.MilestoneChangedEvent += OnMilestoneChanged;
        _uiManager.OnContextualPanelHiddenEvent += OnContextualPanelHidden;
        _uiManager.PlayCharacterClickedEvent += OnPlayCharacterClicked;

        _pathVisualizer = new PathVisualizer(GetComponent<LineRenderer>(),
            _sampleSpacing, _sampleDistance, _projSampleDistance,
            _renderYOffset, _maxPoints);
        _pathVisualizer.Initialize();
    }

    void Update()
    {
        if (_gameManager.PlayableCharacter != null &&
            _gameManager.PlayableCharacter.IsBeingControlled &&
            _currentTour != null && !_currentTour.IsCompleted)
        {
            _pathVisualizer.UpdatePath();
        }
        else
        {
            _pathVisualizer.Clear();
        }
    }

    void OnDisable()
    {
        // Let the visualizer unsubscribe from ProgressManager and cleanup
        _pathVisualizer.Deinitialize();

        // Unsubscribe from events
        _progressManager.MilestoneChangedEvent -= OnMilestoneChanged;
        _uiManager.OnContextualPanelHiddenEvent -= OnContextualPanelHidden;
        _uiManager.PlayCharacterClickedEvent -= OnPlayCharacterClicked;

        DetachFromCurrentTour();
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
        _currentTour.Reset();
        _currentTour.OnVisitedPOIEvent += OnTourPOIVisited;
        _currentTour.OnNextPOIChangeEvent += OnTourNextPOIChange;
    }

    void DetachFromCurrentTour()
    {
        if (_currentTour == null) return;

        _currentTour.OnVisitedPOIEvent -= OnTourPOIVisited;
        _currentTour.OnNextPOIChangeEvent -= OnTourNextPOIChange;
        _currentTour.EndTour();
        _currentTour = null;
        _nextPOI = null;
    }
    #endregion

    #region CALLBACK METHODS
    void OnMilestoneChanged(MilestoneMapping milestoneMapping)
    {
        AttachToTour(milestoneMapping?.Tour);
    }

    void OnTourPOIVisited(PointOfInterest poi)
    {
        TourPOIVisitedEvent?.Invoke(poi);
    }

    void OnTourNextPOIChange(PointOfInterest poi)
    {
        OnTourNextPOIChangeEvent?.Invoke(poi);
        _nextPOI = poi;
    }

    void OnContextualPanelHidden()
    {
        // Invoke completed event if tour is completed
        if (_currentTour != null && _currentTour.IsCompleted)
        {
            OnTourCompletedEvent?.Invoke(_currentTour);
            _soundManager.PlayTourEndSFX();
            DetachFromCurrentTour();
        }
    }

    void OnPlayCharacterClicked()
    {
        if (_currentTour != null)
        {
            _currentTour.StartTour();
            _soundManager.PlayTourStartSFX();
        }
    }
    #endregion
}

