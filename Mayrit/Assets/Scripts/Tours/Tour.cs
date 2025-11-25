using System;
using System.Collections.Generic;
using UnityEngine;

public class Tour : MonoBehaviour
{
    #region PROPERTY HELPERS
    public PointOfInterest NextPOI => _nextPOI;
    #endregion

    #region EDITOR PROPERTIES
    [SerializeField] PointOfInterest _nextPOI;
    [Tooltip("Ordered POIs for this tour")]
    [SerializeField] List<PointOfInterest> _pointsOfInterest = new();
    #endregion

    #region INTERNAL PROPERTIES
    public event Action<PointOfInterest> OnVisitedPOIEvent;
    public event Action<PointOfInterest> OnNextPOIChangeEvent;
    public event Action<Tour> OnCompletedEvent;

    int _currentPOIindex = -1;
    #endregion

    #region PUBLIC METHODS
    public void StartTour()
    {
        Reset();
        Activate();
        UpdateNextPOI();
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
            OnCompletedEvent?.Invoke(this);
            Deactivate();
            return;
        }

        _nextPOI = GetPOIFromList(_currentPOIindex);
        AttachToPOI(_nextPOI);

        OnNextPOIChangeEvent?.Invoke(_nextPOI);
    }

    PointOfInterest GetPOIFromList(int index)
    {
        return (index >= 0 && index < _pointsOfInterest.Count) ?
            _pointsOfInterest[index] :
            null;
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
        Reset();
    }

    void Reset()
    {
        _currentPOIindex = -1;
        ResetPOIs();
    }

    void ResetPOIs()
    {
        foreach (PointOfInterest point in _pointsOfInterest)
            if (point != null) point.Deactivate();
    }
    #endregion

    #region EVENT METHODS
    void OnPOIVisited(PointOfInterest poi)
    {
        OnVisitedPOIEvent?.Invoke(poi);
        UpdateNextPOI();
    }
    #endregion
}

