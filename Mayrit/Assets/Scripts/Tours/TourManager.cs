using System;
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class TourManager : Singleton<TourManager>
{
    #region PROPERTY HELPERS
    public Tour CurrentTour => _currentTour;
    #endregion

    #region EDITOR PROPERTIES
    [Header("Tour manager")]
    [SerializeField] Tour _currentTour;
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
    public event Action<PointOfInterest> OnTourPOIVisitedEvent;
    public event Action<PointOfInterest> OnTourNextPOIChangeEvent;
    public event Action<Tour> OnTourCompletedEvent;

    PathVisualizer _pathVisualizer;
    #endregion

    #region MONOBEHAVIOUR
    void Start()
    {
        _pathVisualizer = new PathVisualizer(GetComponent<LineRenderer>(),
            _sampleSpacing, _sampleDistance, _projSampleDistance,
            _renderYOffset, _maxPoints);
        _pathVisualizer.Initialize();

        // Subscribe to ProgressManager milestone changes to track active tour
        ProgressManager.Instance.OnMilestoneChangedEvent += OnMilestoneChanged;
    }

    void Update()
    {
        if (CameraManager.Instance.IsInThirdPersonState)
            _pathVisualizer.UpdatePath();
        else
            _pathVisualizer.Clear();
    }

    void OnDisable()
    {
        // Let the visualizer unsubscribe from ProgressManager and cleanup
        _pathVisualizer.Deinitialize();

        if (ProgressManager.Instance != null)
            ProgressManager.Instance.OnMilestoneChangedEvent -= OnMilestoneChanged;

        if (_currentTour != null)
            DetachFromTour(_currentTour);
    }
    #endregion

    #region PRIVATE METHODS
    // Attach to a tour managed by ProgressManager
    void AttachToTour(Tour tour)
    {
        if (tour == null) return;

        // Detach previous
        if (_currentTour != null && _currentTour != tour)
            DetachFromTour(_currentTour);

        _currentTour = tour;
        _currentTour.OnVisitedPOIEvent += OnTourPOIVisited;
        _currentTour.OnNextPOIChangeEvent += OnTourNextPOIChange;
        _currentTour.OnCompletedEvent += OnTourCompleted;
    }

    void DetachFromTour(Tour tour)
    {
        if (tour == null) return;

        tour.OnVisitedPOIEvent -= OnTourPOIVisited;
        tour.OnNextPOIChangeEvent -= OnTourNextPOIChange;
        tour.OnCompletedEvent -= OnTourCompleted;
        _currentTour = null;
    }
    #endregion

    #region EVENT METHODS
    void OnMilestoneChanged(MilestoneMapping milestoneMapping)
    {
        AttachToTour(milestoneMapping?.Tour);
        _currentTour.StartTour();
    }

    void OnTourPOIVisited(PointOfInterest poi)
    {
        OnTourPOIVisitedEvent?.Invoke(poi);
    }

    void OnTourNextPOIChange(PointOfInterest poi)
    {
        OnTourNextPOIChangeEvent?.Invoke(poi);
    }

    void OnTourCompleted(Tour tour)
    {
        OnTourCompletedEvent?.Invoke(tour);
    }
    #endregion
}

