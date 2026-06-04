using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

public class Tour : MonoBehaviour
{
    #region PROPERTY HELPERS
    public DataSO Data => _data;
    public bool IsCompleted => _isCompleted;
    public bool HasBeenCompleted => _hasBeenCompleted;
    public TourStop NextStop => _stops[_nextValidStopIdx];
    public TourStop LastStopInList => GetTourStopFromList(_stops.Count - 1);
    public int VisitedStopsCount => _visitedStopsCount;
    public int TotalStopsCount => _totalValidStopsCount;
    #endregion

    #region EDITOR PROPERTIES
    [Tooltip("Information associated with this tour")]
    [SerializeField] DataSO _data;

    [Header("Tour Settings")]
    [SerializeField] bool _isCompleted = false;
    [SerializeField] bool _hasBeenCompleted = false;
    [SerializeField] int _currentStopIdx = 0;
    [Tooltip("Next TourStop to visit in the tour")]
    [SerializeField] TourStop _nextStop;
    [Tooltip("Ordered TourStops for this tour")]
    [SerializeField] List<TourStop> _stops = new();
    [SerializeField] int _totalValidStopsCount = 0;
    [SerializeField] int _visitedStopsCount = 0;
    [SerializeField] int _nextValidStopIdx = 0;
    #endregion

    #region INTERNAL PROPERTIES
    public event Action<TourStop> OnVisitedTourStopEvent;
    public event Action<TourStop> OnNextTourStopChangeEvent;
    public event Action OnTourCompletedEvent;
    #endregion

    #region LIFE CYCLE

    void Awake()
    {
        _stops = new List<TourStop>(GetComponentsInChildren<TourStop>());

        _totalValidStopsCount = 0;
        foreach (TourStop stop in _stops)
        {
            if (stop.Data != null)
                _totalValidStopsCount++;
        }

        ServiceLocator.Instance.Register(this);
    }

    void OnDisable()
    {
        ServiceLocator.Instance.Unregister(this);
    }
    #endregion

    #region PUBLIC METHODS
    public void StartTour()
    {
        Activate();

        if (!_isCompleted)
            SetNextTourStop();
    }

    public void Reset()
    {
        _isCompleted = false;
        _currentStopIdx = 0;
        _visitedStopsCount = 0;
        ResetTourStops();
    }

    public void MarkAsCompleted()
    {
        _isCompleted = true;
        _hasBeenCompleted = true;
        _visitedStopsCount = _totalValidStopsCount;
    }
    #endregion

    #region PRIVATE METHODS
    void UpdateNextStop()
    {
        _nextStop = GetTourStopFromList(_currentStopIdx);

        for (int i = _currentStopIdx; i < _stops.Count; i++)
        {
            TourStop stop = GetTourStopFromList(i);
            if (stop.Data != null)
            {
                if (!stop.IsVisited)
                {
                    _nextValidStopIdx = i;
                    break;
                }
            }
        }

        if (_nextStop != null)
            DetachFromTourStop(_nextStop);

        _currentStopIdx++;

        // All TourStops visited
        if (_currentStopIdx >= _stops.Count)
            TourCompleted();
        else
            SetNextTourStop();
    }

    TourStop GetTourStopFromList(int index)
    {
        return (index >= 0 && index < _stops.Count) ?
            _stops[index] :
            null;
    }

    void SetNextTourStop()
    {
        _nextStop = GetTourStopFromList(_currentStopIdx);

        if (_nextStop == null)
        {
            Debug.LogWarning($"[Tour] Next tour stop is null at index {_currentStopIdx}");
            return;
        }

        AttachToTourStop(_nextStop);
        OnNextTourStopChangeEvent?.Invoke(_nextStop);
    }

    void AttachToTourStop(TourStop tourStop)
    {
        DetachFromTourStop(_nextStop);

        if (tourStop != null)
        {
            tourStop.OnVisitedEvent += OnTourStopVisited;
            tourStop.Activate();
        }
    }

    void DetachFromTourStop(TourStop tourStop)
    {
        if (tourStop != null)
        {
            tourStop.OnVisitedEvent -= OnTourStopVisited;
            tourStop.Deactivate();
        }
    }

    void Activate()
    {
        transform.gameObject.SetActive(true);
    }

    void Deactivate()
    {
        transform.gameObject.SetActive(false);
    }

    void ResetTourStops()
    {
        foreach (TourStop stop in _stops)
            if (stop != null) stop.Reset();
    }

    void TourCompleted()
    {
        _isCompleted = true;
        _hasBeenCompleted = true;
        _nextStop = null;
        OnTourCompletedEvent?.Invoke();
    }
    #endregion

    #region CALLBACK METHODS
    void OnTourStopVisited(TourStop tourStop)
    {
        if (tourStop.Data != null)
            _visitedStopsCount++;

        UpdateNextStop();
        OnVisitedTourStopEvent?.Invoke(tourStop);
    }
    #endregion
}

