using System;
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class TourManager : Singleton<TourManager>
{
    #region PROPERTY HELPERS
    public Tour CurrentTour =>
    (_currentTourIndex >= 0 && _currentTourIndex < _tours.Count) ?
        _tours[_currentTourIndex] :
        null;
    #endregion

    #region EDITOR PROPERTIES
    [Header("Milestones Tours")]
    [Tooltip("Ordered tours as milestones order")]
    public List<Tour> _tours = new();

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

    int _currentTourIndex = -1;
    public event Action OnTourStartedEvent;
    public event Action OnAllToursCompletedEvent;
    public event Action OnTourChangedEvent;
    public event Action<PointOfInterest> OnNextPOIChangedEvent;
    #endregion

    #region MONOBEHAVIOUR
    void Start()
    {
        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        _pathVisualizer = new(lineRenderer, _colorGradient, _lineWidth, _useNavMesh, _maxCorners);
        _pathVisualizer.Initialize();

        Reset();
        NextTour();
    }

    void Update()
    {
        _pathVisualizer.UpdatePath();
    }
    #endregion

    #region PUBLIC METHODS
    public void Reset()
    {
        _currentTourIndex = -1;
        ResetTours();
    }
    #endregion

    #region PRIVATE METHODS
    void NextTour()
    {
        // Handle last tour
        if (CurrentTour != null)
        {
            CurrentTour.OnTourCompletedEvent -= OnTourCompleted;
            CurrentTour.OnNextPOIChangedEvent -= OnNextPOIChanged;
            CurrentTour.Deactivate();
        }

        _currentTourIndex++;

        // All tours visited
        if (_currentTourIndex >= _tours.Count)
        {
            OnAllToursCompletedEvent?.Invoke();
            Reset();
            return;
        }

        // Handle new tour
        if (CurrentTour != null)
        {
            CurrentTour.OnTourCompletedEvent += OnTourCompleted;
            CurrentTour.OnNextPOIChangedEvent += OnNextPOIChanged;
            CurrentTour.Activate();
            CurrentTour.StartTour();
            OnTourStartedEvent?.Invoke();
        }

        OnTourChangedEvent?.Invoke();
    }

    void ResetTours()
    {
        foreach (Tour tour in _tours)
            if (tour != null) tour.Reset();
    }
    #endregion

    #region EVENT METHODS
    void OnTourCompleted()
    {
        NextTour();
    }

    void OnNextPOIChanged(PointOfInterest nextPOI)
    {
        OnNextPOIChangedEvent?.Invoke(nextPOI);
    }
    #endregion
}

