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
    public TourStop NextTourStop => _nextTourStop;
    public TourStop LastTourStopinList => GetTourStopFromList(_tourStops.Count - 1);
    #endregion

    #region EDITOR PROPERTIES
    [Tooltip("Information associated with this tour")]
    [SerializeField] DataSO _data;

    [Header("Tour Settings")]
    [SerializeField] bool _isCompleted = false;
    [SerializeField] bool _hasBeenCompleted = false;
    [SerializeField] int _currentTourStopIdx = 0;
    [Tooltip("Next TourStop to visit in the tour")]
    [SerializeField] TourStop _nextTourStop;
    [Tooltip("Ordered TourStops for this tour")]
    [SerializeField] List<TourStop> _tourStops = new();
    #endregion

    #region INTERNAL PROPERTIES
    public event Action<TourStop> OnVisitedTourStopEvent;
    public event Action<TourStop> OnNextTourStopChangeEvent;
    public event Action OnTourCompletedEvent;

    //ProgressManager _progressManager;
    #endregion

    #region LIFE CYCLE
    // TODO: remove eventually
    // void OnEnable()
    // {
    //     SubscribeToRuntimeEvents();
    // }
    //     void OnValidate()
    //     {
    // #if UNITY_EDITOR
    //         if (!Application.isPlaying)
    //             SubscribeToRuntimeEvents();
    // #endif
    //     }

    void Awake()
    {
        ServiceLocator.Instance.Register(this);

        _tourStops = new List<TourStop>(GetComponentsInChildren<TourStop>());
    }

    void OnDisable()
    {
        //UnsubscribeFromRuntimeEvents();
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
        _currentTourStopIdx = 0;
        ResetTourStops();
    }

    public void MarkAsCompleted()
    {
        _isCompleted = true;
        _hasBeenCompleted = true;
    }
    #endregion

    #region PRIVATE METHODS
    void UpdateNextTourStop()
    {
        _nextTourStop = GetTourStopFromList(_currentTourStopIdx);

        if (_nextTourStop != null)
            DetachFromTourStop(_nextTourStop);

        _currentTourStopIdx++;

        // All TourStops visited
        if (_currentTourStopIdx >= _tourStops.Count)
        {
            TourCompleted();
            return;
        }
        SetNextTourStop();
    }

    TourStop GetTourStopFromList(int index)
    {
        return (index >= 0 && index < _tourStops.Count) ?
            _tourStops[index] :
            null;
    }

    void SetNextTourStop()
    {
        _nextTourStop = GetTourStopFromList(_currentTourStopIdx);

        if (_nextTourStop == null)
        {
            Debug.LogWarning($"[Tour] Next tour stop is null at index {_currentTourStopIdx}");
            return;
        }

        AttachToTourStop(_nextTourStop);
        OnNextTourStopChangeEvent?.Invoke(_nextTourStop);
    }

    void AttachToTourStop(TourStop tourStop)
    {
        DetachFromTourStop(_nextTourStop);

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
        foreach (TourStop stop in _tourStops)
            if (stop != null) stop.Reset();
    }

    void TourCompleted()
    {
        _isCompleted = true;
        _hasBeenCompleted = true;
        _nextTourStop = null;
        OnTourCompletedEvent?.Invoke();
    }
    #endregion

    #region CALLBACK METHODS
    void OnTourStopVisited(TourStop tourStop)
    {
        OnVisitedTourStopEvent?.Invoke(tourStop);
        UpdateNextTourStop();
    }

    // TODO: remove eventually
    // void OnMilestoneChanged(Milestone_DataSO milestoneMapping)
    // {
    //     if (milestoneMapping.Tour == this)
    //         Activate();
    //     else
    //         Deactivate();
    // }
    #endregion

    #region EDITOR UPDATES
    // TODO: remove eventually
    // void SubscribeToRuntimeEvents()
    // {
    //     _progressManager = FindAnyObjectByType<ProgressManager>();

    //     if (_progressManager != null)
    //     {
    //         _progressManager.MilestoneChangedEvent += OnMilestoneChanged;
    //         //_progressManager.OnEditorUpdateChangedEvent += OnEditorUpdateChanged;
    //     }
    // }

    // void UnsubscribeFromRuntimeEvents()
    // {
    //     _progressManager = FindAnyObjectByType<ProgressManager>();

    //     if (_progressManager != null)
    //     {
    //         _progressManager.MilestoneChangedEvent -= OnMilestoneChanged;
    //         //_progressManager.OnEditorUpdateChangedEvent -= OnEditorUpdateChanged;
    //     }
    // }

    //     void OnEditorUpdateChanged(bool updateInEditor)
    //     {
    // #if UNITY_EDITOR
    //         if (Application.isPlaying)
    //             return;

    //         if (this == null) return;

    //         // Not updated through editor
    //         if (!updateInEditor)
    //             // All tours active
    //             Activate();
    //         else
    //         {
    //             // Only active if corresponding to current milestone
    //             if (ServiceLocator.Instance.Get<Tour>() == this)
    //                 Activate();
    //             else
    //                 Deactivate();
    //         }
    // #endif
    //     }
    #endregion
}

