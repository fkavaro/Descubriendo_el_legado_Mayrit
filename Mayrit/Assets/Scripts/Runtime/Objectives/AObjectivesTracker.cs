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
    [SerializeField] protected TObject _currentObjective;
    [SerializeField] protected TObject _currentValidObjective;
    [SerializeField] protected int _currentIdx = -1;
    [SerializeField] protected int _currentValidIdx = -1;
    [SerializeField] protected int _totalCount = 0;
    [SerializeField] protected int _reachedCount = 0;
    #endregion

    #region INTERNAL PROPERTIES
    public event Action<TObject> OnObjectiveReachedEvent;
    public event Action OnCompletedEvent;

    GameManager _gameManager;
    #endregion

    #region ACCESSORS
    public TObject CurrentObjective => _currentObjective;
    public TObject CurrentValidObjective => _currentValidObjective;
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

    protected virtual void Start()
    {
        _gameManager = ServiceLocator.Instance.Get<GameManager>();
        _gameManager.StateChangedEvent += OnGameStateChanged;
    }

    protected virtual void OnDisable()
    {
        _gameManager.StateChangedEvent -= OnGameStateChanged;
        ServiceLocator.Instance.Unregister((TTracker)this);
    }
    #endregion

    #region PUBLIC METHODS
    public virtual void Reset()
    {
        _isCompleted = false;

        foreach (var obj in _objectives)
            obj.Reset();

        UpdateStateAndProgress();
    }

    public virtual void SyncWithReachedObjectives(HashSet<TData> allReachedCollectiblesHash)
    {
        foreach (var obj in _objectives)
            if (obj.Data != null && allReachedCollectiblesHash.Contains(obj.Data))
                obj.CompleteAndUpdateVisuals();

        UpdateStateAndProgress();
    }

    public virtual void Complete()
    {
        _isCompleted = true;
        _reachedCount = _totalCount;
        foreach (var obj in _objectives)
            obj.CompleteAndUpdateVisuals();
    }
    #endregion

    void OnGameStateChanged()
    {
        if (_gameManager.IsInThirdPersonState)
            _currentObjective?.UpdateModel();
    }

    #region PRIVATE METHODS
    protected void HandleObjectReached(TObject reachedObj)
    {
        OnObjectiveReachedAction(reachedObj);

        UpdateStateAndProgress();

        OnObjectiveReachedEvent?.Invoke(reachedObj);

        if (_isCompleted)
            OnCompletedEvent?.Invoke();
    }

    void UpdateStateAndProgress()
    {
        _currentIdx = -1;
        _currentValidIdx = -1;
        _reachedCount = 0;

        if (_currentObjective != null)
            _currentObjective.OnReachedEvent -= HandleObjectReached;
        if (_currentValidObjective != null)
            _currentValidObjective.OnReachedEvent -= HandleObjectReached;

        bool foundNextIdx = false;
        bool foundNextValidIdx = false;

        for (int i = 0; i < _objectives.Count; i++)
        {
            var obj = _objectives[i];

            if (obj.IsReached)
            {
                if (obj.Data != null)
                    _reachedCount++;
            }
            else
            {
                if (!foundNextIdx)
                {
                    _currentIdx = i;
                    _currentObjective = _objectives[_currentIdx];
                    foundNextIdx = true;
                }

                if (!foundNextValidIdx && obj.Data != null)
                {
                    _currentValidIdx = i;
                    _currentValidObjective = _objectives[_currentValidIdx];
                    foundNextValidIdx = true;
                }
            }
        }

        _isCompleted = _reachedCount >= _totalCount;

        if (_isCompleted)
        {
            _currentObjective = null;
            _currentValidObjective = null;
        }
        else
        {
            _currentObjective.Reset();
            _currentObjective.OnReachedEvent += HandleObjectReached;
            _currentValidObjective.Reset();
            _currentValidObjective.OnReachedEvent += HandleObjectReached;
        }
    }
    #endregion

    #region PROTECTED VIRTUAL METHODS
    protected virtual void OnObjectiveReachedAction(TObject reachedObj)
    {
        reachedObj.CompleteAndUpdateVisuals();
    }
    #endregion
}