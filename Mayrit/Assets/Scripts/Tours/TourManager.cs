using System;
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class TourManager : Singleton<TourManager>
{


    #region EDITOR PROPERTIES
    [Header("Path Visualizer Settings")]
    [Tooltip("Use NavMesh.CalculatePath when available; otherwise fall back to a straight-line path.")]
    public bool _useNavMesh = true;

    [Tooltip("Width of the line")]
    public float _lineWidth = 0.1f;

    [Tooltip("Maximum number of corners to display (safety cap)")]
    public int _maxCorners = 512;
    public Gradient _colorGradient;
    #endregion

    #region INTERNAL PROPERTIES
    PathVisualizer _pathVisualizer;
    Tour _currentTour = null;
    public event Action OnTourCompletedEvent;
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
        _pathVisualizer.UpdatePath();
    }

    void OnDisable()
    {
        // Let the visualizer unsubscribe from ProgressManager and cleanup
        _pathVisualizer?.Deinitialize();

        if (ProgressManager.Instance != null)
            ProgressManager.Instance.OnMilestoneChangedEvent -= OnMilestoneChanged;

        if (_currentTour != null)
            DetachFromTour(_currentTour);
    }
    #endregion

    #region PUBLIC METHODS
    public void Reset()
    {
        // Detach from any current tour and reset index
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
        _currentTour.OnTourCompletedEvent += OnTourCompleted;
        _currentTour.StartTour();
    }

    void DetachFromTour(Tour tour)
    {
        if (tour == null) return;
        tour.OnTourCompletedEvent -= OnTourCompleted;
        _currentTour = null;
    }
    #endregion

    #region EVENT METHODS
    void OnTourCompleted()
    {
        OnTourCompletedEvent?.Invoke();
    }

    void OnMilestoneChanged(MilestoneMapping milestoneMapping)
    {
        AttachToTour(ProgressManager.Instance.CurrentMilestoneMapping?.Tour);
    }
    #endregion
}

