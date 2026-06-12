using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class CollectiblesManager : MonoBehaviour
{
    #region PROPERTY HELPERS
    public CollectiblesTracker CurrentTracker => _currentTracker;
    public Collectible CurrentCollectible => _currentTracker != null ? _currentTracker.CurrentValidObjective : null;
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
    private readonly Dictionary<int, CollectibleSO> _idToCollectibleMap = new();

    ScenesController _scenesController;
    SoundSystem _soundSystem;
    #endregion

    #region LIFE CYCLE
    void Awake()
    {
        _allTotalCollectiblesHash = new HashSet<CollectibleSO>(_allCollectiblesSOs);

        _idToCollectibleMap.Clear();
        foreach (var so in _allCollectiblesSOs)
        {
            if (so != null && !_idToCollectibleMap.ContainsKey(so.ID))
                _idToCollectibleMap.Add(so.ID, so);
        }

        ServiceLocator.Instance.Register(this);
    }

    void Start()
    {
        _scenesController = ServiceLocator.Instance.Get<ScenesController>();
        _soundSystem = ServiceLocator.Instance.Get<SoundSystem>();

        _scenesController.SceneLoadedPartiallyEvent += OnSceneLoadedPartially;
    }

    void OnDisable()
    {
        ServiceLocator.Instance.Unregister(this);
        DetachFromCurrentTracker();

        if (_scenesController != null)
            _scenesController.SceneLoadedPartiallyEvent -= OnSceneLoadedPartially;
    }
    #endregion

    #region CALLBACK METHODS
    void OnSceneLoadedPartially(SceneDatabase.SceneType type, SceneDatabase.SceneName name)
    {
        if (type == SceneDatabase.SceneType.Milestone)
        {
            LoadSavedCollectiblesData();
            AttachToNewTracker(ServiceLocator.Instance.Get<CollectiblesTracker>());
        }
    }

    void OnCollectibleFound(Collectible collectible)
    {
        var info = collectible.Data;
        if (info == null) return;

        if (_allFoundCollectiblesHash.Add(info))
        {
            _allFoundCollectiblesSOs.Add(info);
            GameSaveSystem.SaveFoundCollectible(info.ID);
            _soundSystem.PlayTourEndSFX();
        }
        else
            Debug.LogWarning($"[CollectiblesManager] {info.Data.Header} had been already found.");

        OnCollectibleFoundEvent?.Invoke(collectible);
    }
    #endregion

    #region PRIVATE METHODS
    void LoadSavedCollectiblesData()
    {
        _allFoundCollectiblesHash.Clear();
        _allFoundCollectiblesSOs.Clear();

        List<int> savedIds = GameSaveSystem.LoadFoundCollectibles();

        foreach (int id in savedIds)
        {
            if (_idToCollectibleMap.TryGetValue(id, out CollectibleSO foundSO))
            {
                if (_allFoundCollectiblesHash.Add(foundSO))
                    _allFoundCollectiblesSOs.Add(foundSO);
            }
        }
    }

    void AttachToNewTracker(CollectiblesTracker tracker)
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

        DetachFromCurrentTracker();

        _currentTracker = tracker;

        _currentTracker.SyncWithReachedObjectives(_allFoundCollectiblesHash);
        _currentTracker.OnObjectiveReachedEvent += OnCollectibleFound;
    }

    void DetachFromCurrentTracker()
    {
        if (_currentTracker == null) return;

        _currentTracker.OnObjectiveReachedEvent -= OnCollectibleFound;
    }
    #endregion
}