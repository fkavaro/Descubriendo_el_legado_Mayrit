using UnityEngine;
using System;
using System.Collections.Generic;

public abstract class AObjectivesTracker<TTracker, TObject, TData> : MonoBehaviour
    where TTracker : AObjectivesTracker<TTracker, TObject, TData>
    where TObject : AObjective<TObject, TData>
{
    #region EDITOR PROPERTIES
    [Header("Tracker State")]
    [SerializeField] protected bool _isCompleted = false;
    [SerializeField] protected List<TObject> _objectives = new();

    [Header("Progress")]
    [SerializeField] protected int _currentIdx = 0;
    [SerializeField] protected int _totalCount = 0;
    [SerializeField] protected int _reachedCount = 0;
    #endregion

    #region INTERNAL PROPERTIES
    public event Action<TObject> OnObjectiveReachedEvent;
    public event Action OnCompletedEvent;
    #endregion

    #region ACCESSORS
    public TObject CurrentObjective => (_currentIdx >= 0 && _currentIdx < _objectives.Count) ? _objectives[_currentIdx] : null;
    public TObject CurrentValidObjective
    {
        get
        {
            int tempIdx = _currentIdx;
            while (tempIdx < _objectives.Count)
            {
                if (_objectives[tempIdx].Data != null)
                    return _objectives[tempIdx];
                else
                    tempIdx++;
            }
            return null;
        }
    }
    public bool IsCompleted => _isCompleted;
    public int ReachedCount => _reachedCount;
    public int TotalCount => _totalCount;
    #endregion

    #region LIFE CYCLE
    protected virtual void Awake()
    {
        _objectives = new List<TObject>(GetComponentsInChildren<TObject>(true));

        foreach (var obj in _objectives)
        {
            if (obj.Data != null)
                _totalCount++;
        }

        ServiceLocator.Instance.Register((TTracker)this);
    }

    protected virtual void OnEnable()
    {
        foreach (var obj in _objectives)
            obj.OnReachedEvent += HandleObjectReached;
    }

    protected virtual void OnDisable()
    {
        foreach (var obj in _objectives)
            if (obj != null) obj.OnReachedEvent -= HandleObjectReached;

        ServiceLocator.Instance.Unregister((TTracker)this);
    }
    #endregion

    #region PUBLIC METHODS
    public virtual void Reset()
    {
        _isCompleted = false;
        _currentIdx = 0;
        _reachedCount = 0;
        _objectives[_currentIdx].Reset();
    }


    public virtual void Complete()
    {
        _isCompleted = true;
        _reachedCount = _totalCount;
        foreach (var obj in _objectives) obj.Complete();
    }
    #endregion

    #region LOGIC
    protected virtual void HandleObjectReached(TObject obj)
    {
        if (obj.Data != null)
            _reachedCount++;
        _currentIdx++;

        OnObjectiveReachedEvent?.Invoke(obj);

        if (_reachedCount >= _totalCount)
        {
            Complete();
            OnCompletedEvent?.Invoke();
        }
        else
            _objectives[_currentIdx].Reset();
    }

    #endregion
}