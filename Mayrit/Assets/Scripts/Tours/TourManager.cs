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

    [Header("Path Visualizer Settings")]
    [Tooltip("Use NavMesh.CalculatePath when available; otherwise fall back to a straight-line path.")]
    public bool _useNavMesh = true;

    [Tooltip("Width of the line")]
    [SerializeField] float _lineWidth = 0.1f;

    [Tooltip("Maximum number of corners to display (safety cap)")]
    [SerializeField] int _maxCorners = 512;
    [SerializeField] Gradient _colorGradient;
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
        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        _pathVisualizer = new(lineRenderer, _colorGradient, _lineWidth, _useNavMesh, _maxCorners);
        _pathVisualizer.Initialize();

        // Subscribe to ProgressManager milestone changes to track active tour
        ProgressManager.Instance.OnMilestoneChangedEvent += OnMilestoneChanged;
    }

    void Update()
    {
        if (CameraManager.Instance.IsInThirdPersonState)
            _pathVisualizer.UpdatePath();
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

