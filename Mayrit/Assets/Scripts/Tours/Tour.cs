using System;
using System.Collections.Generic;
using UnityEngine;

public class Tour : MonoBehaviour
{
    #region PROPERTY HELPERS
    public DataSO Data => _data;
    public bool IsCompleted => _isCompleted;
    public PointOfInterest NextPOI => _nextPOI;
    #endregion

    #region EDITOR PROPERTIES
    [Tooltip("Information associated with this tour")]
    [SerializeField] DataSO _data;

    [Header("Tour Settings")]
    [SerializeField] bool _isCompleted;
    [SerializeField] int _currentPOIindex = 0;
    [Tooltip("Next POI to visit in the tour")]
    [SerializeField] PointOfInterest _nextPOI;
    [Tooltip("Ordered POIs for this tour")]
    [SerializeField] List<PointOfInterest> _pointsOfInterest = new();
    #endregion

    #region INTERNAL PROPERTIES
    public event Action<PointOfInterest> OnVisitedPOIEvent;
    public event Action<PointOfInterest> OnNextPOIChangeEvent;

    ProgressManager _progressManager;
    #endregion

    #region LIFE CYCLE
    void OnEnable()
    {
        SubscribeToRuntimeEvents();
    }

    void OnValidate()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            SubscribeToRuntimeEvents();
#endif
    }

    void OnDestroy()
    {
        UnsubscribeFromRuntimeEvents();
    }
    #endregion

    #region PUBLIC METHODS
    public void Reset()
    {
        _isCompleted = false;
        _currentPOIindex = 0;
        ResetPOIs();
    }

    public void StartTour()
    {
        Activate();
        SetNextPOI();
    }

    public void EndTour()
    {
        Deactivate();
    }
    #endregion

    #region PRIVATE METHODS
    void UpdateNextPOI()
    {
        _nextPOI = GetPOIFromList(_currentPOIindex);

        if (_nextPOI != null)
            DetachFromPOI(_nextPOI);

        _currentPOIindex++;

        // All POIs visited
        if (_currentPOIindex >= _pointsOfInterest.Count)
        {
            TourCompleted();
            return;
        }

        SetNextPOI();
    }

    PointOfInterest GetPOIFromList(int index)
    {
        return (index >= 0 && index < _pointsOfInterest.Count) ?
            _pointsOfInterest[index] :
            null;
    }

    void SetNextPOI()
    {
        _nextPOI = GetPOIFromList(_currentPOIindex);
        AttachToPOI(_nextPOI);
        OnNextPOIChangeEvent?.Invoke(_nextPOI);
    }

    void AttachToPOI(PointOfInterest poi)
    {
        if (poi != null)
        {
            poi.OnVisitedEvent += OnPOIVisited;
            poi.Activate();
        }
    }

    void DetachFromPOI(PointOfInterest poi)
    {
        if (poi != null)
        {
            poi.OnVisitedEvent -= OnPOIVisited;
            poi.Deactivate();
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

    void ResetPOIs()
    {
        foreach (PointOfInterest point in _pointsOfInterest)
            if (point != null) point.Reset();
    }

    void TourCompleted()
    {
        _isCompleted = true;
        _nextPOI = null;
    }
    #endregion

    #region CALLBACK METHODS
    void OnPOIVisited(PointOfInterest poi)
    {
        OnVisitedPOIEvent?.Invoke(poi);
        UpdateNextPOI();
    }
    #endregion

    #region EDITOR UPDATES
    void SubscribeToRuntimeEvents()
    {
        _progressManager = FindAnyObjectByType<ProgressManager>();

        if (_progressManager != null)
        {
            _progressManager.OnMilestoneChangedEvent += OnMilestoneChanged;
            _progressManager.OnEditorUpdateChangedEvent += OnEditorUpdateChanged;
        }
    }

    void UnsubscribeFromRuntimeEvents()
    {
        _progressManager = FindAnyObjectByType<ProgressManager>();

        if (_progressManager != null)
        {
            _progressManager.OnMilestoneChangedEvent -= OnMilestoneChanged;
            _progressManager.OnEditorUpdateChangedEvent -= OnEditorUpdateChanged;
        }
    }

    void OnMilestoneChanged(MilestoneMapping milestoneMapping)
    {
        if (milestoneMapping.Tour == this)
            Activate();
        else
            Deactivate();
    }

    void OnEditorUpdateChanged(bool updateInEditor)
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
            return;

        if (this == null) return;

        // Not updated through editor
        if (!updateInEditor)
            // All tours active
            Activate();
        else
        {
            // Only active if corresponding to current milestone
            if (_progressManager.CurrentMilestoneMapping.Tour == this)
                Activate();
            else
                Deactivate();
        }
#endif
    }
    #endregion
}

