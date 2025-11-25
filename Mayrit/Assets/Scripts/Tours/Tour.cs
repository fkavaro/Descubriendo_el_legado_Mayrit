using System;
using System.Collections.Generic;
using UnityEngine;

public class Tour : MonoBehaviour
{
    #region PROPERTY HELPERS
    public PointOfInterest CurrentPOI =>
    (_currentPOIindex >= 0 && _currentPOIindex < _pointsOfInterest.Count) ?
        _pointsOfInterest[_currentPOIindex] :
        null;
    #endregion

    #region EDITOR PROPERTIES
    [Tooltip("Ordered POIs for this tour")]
    public List<PointOfInterest> _pointsOfInterest = new();
    #endregion

    #region INTERNAL PROPERTIES
    public event Action OnTourCompletedEvent;
    public event Action<PointOfInterest> OnNextPOIChangedEvent;

    int _currentPOIindex = -1;
    #endregion

    #region PUBLIC METHODS
    /// <summary>
    /// Resets all POIs and starts the tour from the first POI in its list.
    /// Also invokes the OnTourStarted event.
    /// </summary>
    public void StartTour()
    {
        Reset();
        Activate();
        NextPOI();
    }

    /// <summary>
    /// Resets the tour to its initial state, marking all POIs as unvisited
    /// </summary>
    public void Reset()
    {
        _currentPOIindex = -1;
        ResetPOIs();
    }

    /// <summary>
    /// Activates the tour GameObject.
    /// </summary>
    public void Activate()
    {
        transform.gameObject.SetActive(true);
    }

    /// <summary>
    /// Deactivates the tour GameObject.
    /// </summary>
    public void Deactivate()
    {
        transform.gameObject.SetActive(false);
    }
    #endregion

    #region PRIVATE METHODS
    /// <summary>
    /// Advances to the next POI in the tour. If the last POI has been
    /// visited, invokes the OnTourCompleted event and resets the tour.
    /// </summary>
    void NextPOI()
    {
        // Handle last POI
        if (CurrentPOI != null)
        {
            CurrentPOI.OnVisitedPOIEvent -= OnPOIVisited;
            CurrentPOI.Deactivate();
        }

        _currentPOIindex++;

        // All POIs visited
        if (_currentPOIindex >= _pointsOfInterest.Count)
        {
            OnTourCompletedEvent?.Invoke();
            Reset();
            return;
        }

        // Handle new POI
        if (CurrentPOI != null)
        {
            CurrentPOI.OnVisitedPOIEvent -= OnPOIVisited;
            CurrentPOI.Activate();
        }

        OnNextPOIChangedEvent?.Invoke(CurrentPOI);
    }

    void ResetPOIs()
    {
        foreach (PointOfInterest point in _pointsOfInterest)
            if (point != null) point.Deactivate();
    }
    #endregion

    #region EVENT METHODS
    void OnPOIVisited()
    {
        NextPOI();
    }
    #endregion
}

