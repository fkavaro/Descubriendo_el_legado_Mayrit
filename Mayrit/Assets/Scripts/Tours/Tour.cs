using System;
using System.Collections.Generic;
using UnityEngine;

public class Tour : MonoBehaviour
{
    #region PROPERTY HELPERS
    public PointOfInterest CurrentPOI => _currentPOI;
    #endregion

    #region EDITOR PROPERTIES
    [Tooltip("Ordered POIs for this tour")]
    public List<PointOfInterest> _pointsOfInterest = new();
    #endregion

    #region INTERNAL PROPERTIES
    public event Action<PointOfInterest> OnVisitedPOIEvent;
    public event Action<PointOfInterest> OnNextPOIChangeEvent;
    public event Action<Tour> OnCompletedEvent;

    int _currentPOIindex = -1;
    PointOfInterest _currentPOI;
    #endregion

    #region PUBLIC METHODS
    // TODO called when selecting playable character 
    public void StartTour()
    {
        Reset();
        Activate();
        NextPOI();
    }
    #endregion

    #region PRIVATE METHODS
    void NextPOI()
    {
        _currentPOI = GetPOIFromList(_currentPOIindex);
        DetachFromPOI(_currentPOI);

        _currentPOIindex++;

        // All POIs visited
        if (_currentPOIindex >= _pointsOfInterest.Count)
        {
            OnCompletedEvent?.Invoke(this);
            Deactivate();
            return;
        }

        _currentPOI = GetPOIFromList(_currentPOIindex);
        AttachToPOI(_currentPOI);

        OnNextPOIChangeEvent?.Invoke(_currentPOI);
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
        NextPOI();
    }
    #endregion
}

