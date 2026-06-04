using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class CollectiblesManager : MonoBehaviour
{
    #region PROPERTY HELPERS
    public CollectiblesTracker CurrentTracker => _currentTracker;
    public Collectible NextCollectible => _currentTracker != null ? _currentTracker.CurrentValidObjective : null;
    public int FoundCollectiblesCount => _currentTracker != null ? _currentTracker.ReachedCount : 0;
    public int TotalCollectiblesCount => _currentTracker != null ? _currentTracker.TotalCount : 0;
    public int AllTotalCollectiblesCount => _allCollectiblesSOs.Count;
    public int AllFoundCollectiblesCount => _allFoundCollectiblesSOs.Count;

    #endregion

    #region EDITOR PROPERTIES
    [SerializeField] CollectiblesTracker _currentTracker;
    [SerializeField] List<CollectibleSO> _allCollectiblesSOs = new();
    [SerializeField] List<CollectibleSO> _allFoundCollectiblesSOs = new();
    #endregion

    #region INTERNAL PROPERTIES
    public event Action<Collectible> OnCollectibleFoundEvent;

    private HashSet<CollectibleSO> _allTotalCollectiblesHash;
    private readonly HashSet<CollectibleSO> _allFoundCollectiblesHash = new();

    ScenesController _scenesController;
    SoundManager _soundManager;
    ProgressManager _progressManager;
    #endregion

    #region LIFE CYCLE
    void Awake()
    {
        _allTotalCollectiblesHash = new HashSet<CollectibleSO>(_allCollectiblesSOs);

        ServiceLocator.Instance.Register(this);
    }

    void Start()
    {
        _scenesController = ServiceLocator.Instance.Get<ScenesController>();
        _soundManager = ServiceLocator.Instance.Get<SoundManager>();
        _progressManager = ServiceLocator.Instance.Get<ProgressManager>();

        _scenesController.SceneLoadedPartiallyEvent += OnSceneLoadedPartially;
    }

    void OnDisable()
    {
        ServiceLocator.Instance.Unregister(this);
        DetachFromTracker();
    }
    #endregion

    #region PUBLIC METHODS

    #endregion

    #region PRIVATE METHODS
    void AttachToTracker(CollectiblesTracker tracker)
    {
        if (tracker == null)
        {
            Debug.LogWarning($"[CollectibleManager] Can't attach to null tracker");
            return;
        }
        if (_currentTracker == tracker)
        {
            Debug.LogWarning($"[CollectibleManager] Already attached to this tracker", tracker);
            return;
        }

        // Detach previous
        DetachFromTracker();

        // Update current
        _currentTracker = tracker;
        _currentTracker.Reset();
        _currentTracker.OnObjectiveReachedEvent += OnCollectibleFound;
    }

    void DetachFromTracker()
    {
        if (_currentTracker == null) return;

        _currentTracker.OnObjectiveReachedEvent -= OnCollectibleFound;
    }
    #endregion

    #region CALLBACK METHODS
    void OnSceneLoadedPartially(SceneDatabase.SceneType type, SceneDatabase.SceneName name)
    {
        // Milestone loaded: attach to its tour
        if (type == SceneDatabase.SceneType.Milestone)
        {
            AttachToTracker(ServiceLocator.Instance.Get<CollectiblesTracker>());

            //TODO: Set found collectibles count based on progress on scene load
            //tracker.SetFoundCollectibles = _progressManager.FoundCollectibles;
        }
    }

    private void OnCollectibleFound(Collectible collectible)
    {
        var info = collectible.Data;
        if (info == null) return;

        if (!_allTotalCollectiblesHash.Contains(info))
        {
            Debug.LogWarning($"[CollectiblesManager] {info.Data.Header} no pertenece a la colección global.");
            return;
        }

        if (_allFoundCollectiblesHash.Add(info))
        {
            Debug.Log($"[CollectiblesManager] New collectible found: {info.Data.Header}");

            // TODO: change to custom SFX
            _soundManager.PlayTourEndSFX();
        }
        else
            Debug.LogWarning($"[CollectiblesManager] {info.Data.Header} had been already found.");

        OnCollectibleFoundEvent?.Invoke(collectible);
    }
    #endregion
}
