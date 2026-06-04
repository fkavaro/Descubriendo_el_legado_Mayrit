using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class CollectiblesTracker : MonoBehaviour
{
    #region PROPERTY HELPERS
    public Collectible NextCollectible => _nextCollectible;
    public int FoundCollectiblesCount => _foundCollectiblesCount;
    public int TotalCollectiblesCount => _totalCollectiblesCount;
    #endregion

    #region EDITOR PROPERTIES
    [SerializeField] bool _isCompleted = false;
    [SerializeField] bool _hasBeenCompleted = false;
    [Tooltip("Ordered Collectibles for this tracker")]
    [SerializeField] List<Collectible> _collectibles = new();
    [SerializeField] Collectible _nextCollectible = null;
    [SerializeField] int _foundCollectiblesCount = 0;
    [SerializeField] int _totalCollectiblesCount = 0;
    #endregion

    #region INTERNAL PROPERTIES
    int _nextCollectibleIdx = -1;

    public event Action<Collectible> OnCollectibleFoundEvent;
    #endregion

    #region LIFE CYCLE
    void Awake()
    {
        _collectibles = new List<Collectible>(GetComponentsInChildren<Collectible>());
        _totalCollectiblesCount = _collectibles.Count;
        _foundCollectiblesCount = 0;
        _nextCollectibleIdx = -1;

        foreach (Collectible collectible in _collectibles)
            collectible.OnFoundEvent += OnCollectibleFound;

        ServiceLocator.Instance.Register(this);
    }

    void Start()
    {
        UpdateNextCollectible();
    }

    void OnDisable()
    {
        ServiceLocator.Instance.Unregister(this);
    }
    #endregion

    #region PUBLIC METHODS

    #endregion

    #region PRIVATE METHODS
    void UpdateNextCollectible()
    {
        if (_nextCollectibleIdx >= _collectibles.Count - 1)
            MarkAsCompleted();
        else
        {
            _nextCollectible = _collectibles[++_nextCollectibleIdx];
            _foundCollectiblesCount = _nextCollectibleIdx;
        }
    }

    void MarkAsCompleted()
    {
        _isCompleted = true;
        _hasBeenCompleted = true;
        _foundCollectiblesCount = _totalCollectiblesCount;
        _nextCollectibleIdx = _collectibles.Count;
        _nextCollectible = null;
    }
    #endregion

    #region CALLBACK METHODS
    void OnCollectibleFound(Collectible collectible)
    {
        UpdateNextCollectible();
        OnCollectibleFoundEvent?.Invoke(collectible);
    }
    #endregion
}
