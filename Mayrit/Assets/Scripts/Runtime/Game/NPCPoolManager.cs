using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Pool;

/// <summary>
/// Pool manager for NPCs (Non-Player Characters).
/// </summary>
public class NPCPoolManager : MonoBehaviour
{
    #region EDITOR PROPERTIES
    [Header("Villagers pool")]
    [Tooltip("Names database (ScriptableObject)")]
    public NamesDatabaseSO _namesDatabase;
    public GameObject[] _femaleVillagerPrefabs;
    public GameObject[] _maleVillagerPrefabs;
    [Tooltip("Proportion of villagers that should be female (0..1)"), Range(0f, 1f)]
    public float _femaleRatio = 0.5f;
    [Tooltip("Ratio of active villagers to total population"), Range(0f, 1f)]
    public float _activeVillagersRatio = 0.3f;
    [Tooltip("Maximum number of villager at once")]
    public int _maxActiveVillagers;
    public List<Villager> _activeVillagers = new();

    [Header("Proximity Query")]
    [Tooltip("Layer mask used for Physics overlap queries to find villager colliders")]
    public LayerMask _villagerLayer;
    [Tooltip("Maximum number of collider hits to consider in a single proximity query")]
    public int _maxOverlapResults = 32;
    [Tooltip("Max villagers to spawn or return to pool per frame. Higher = faster recovery after population changes; lower = smoother frame budget.")]
    public int _poolAdjustmentBatchSize = 1;
    #endregion

    #region INTERNAL PROPERTIES
    public ObjectPool<Villager> _villagerPool;

    Collider[] _overlapResults; // Cached buffer for OverlapSphereNonAlloc
    Dictionary<Collider, Villager> _colliderToVillager; // Cache to avoid GetComponent calls on colliders returned by physics queries
    readonly List<Villager> _nearbyBuffer = new(); // Reusable scratch buffer for proximity queries — do not store returned references across frames
    bool _physicsQueryReady; // True when physics overlap queries are usable (cached in Awake)
    readonly Queue<Villager> _pendingRelease = new(); // Villagers that failed setup and must be returned to the pool next frame

    // Dependency Injection
    TownManager _townManager;
    UIManager _uiManager;
    #endregion

    #region LIFE CYCLE
    void OnEnable()
    {
        // NamesDatabase is required for name generation. Disable this manager if not assigned.
        if (_namesDatabase == null)
        {
            Debug.LogError("NPCPoolManager requires a NamesDatabase assigned in the inspector. Disabling NPCPoolManager.");
            enabled = false;
            return;
        }
    }

    void Awake()
    {
        // Pool creation
        _villagerPool = new ObjectPool<Villager>(
            createFunc: CreateVillager,
            actionOnGet: GetVillager,
            actionOnDestroy: (villager) => Destroy(villager.gameObject)
        );

        // Allocate cached overlap buffer (guard against zero/negative inspector value)
        _overlapResults = new Collider[Mathf.Max(1, _maxOverlapResults)];
        _physicsQueryReady = _villagerLayer != 0;
        // Initialize collider->villager cache
        _colliderToVillager = new Dictionary<Collider, Villager>(_maxActiveVillagers > 0 ? _maxActiveVillagers * 2 : 32);

        ServiceLocator.Instance.Register(this);
    }

    void Start()
    {
        // Get dependencies from ServiceLocator
        _townManager = ServiceLocator.Instance.Get<TownManager>();
        _uiManager = ServiceLocator.Instance.Get<UIManager>();

        // Subscribe to town population changes
        _townManager.OnPopulationChanged += OnTownPopulationChanged;
    }

    void Update()
    {
        if (_uiManager.IsInLoadingScreenState)
            return;

        // Drain villagers that failed setup last frame (avoids reentrancy with ObjectPool.Get)
        while (_pendingRelease.Count > 0)
            _villagerPool.Release(_pendingRelease.Dequeue());

        int activeDifference = _maxActiveVillagers - _activeVillagers.Count;
        int batch = Mathf.Min(Mathf.Abs(activeDifference), _poolAdjustmentBatchSize);
        if (activeDifference < 0)
            for (int i = 0; i < batch; i++)
                _villagerPool.Release(_activeVillagers[^1]); // Return last active villager to pool
        else if (activeDifference > 0)
            for (int i = 0; i < batch; i++)
                _villagerPool.Get();
    }

    void OnDisable()
    {
        // Unsubscribe from town population changes (guarded to avoid NullReferenceException during teardown)
        _townManager.OnPopulationChanged -= OnTownPopulationChanged;

        ServiceLocator.Instance.Unregister(this);
    }
    #endregion

    #region PUBLIC METHODS
    /// <summary>
    /// Public method to return a villager instance back to the pool.
    /// </summary>
    public void ReturnVillagerToPool(Villager villager)
    {
        villager.Reset();
        _villagerPool.Release(villager);
        _activeVillagers.Remove(villager);
    }

    /// <summary>
    /// Generic version: returns the first nearby NPC of type T within range (or null).
    /// Filters results to only return NPCs of the specified type.
    /// </summary>
    public T GetAnyNearby<T>(Vector3 position, float range, INPC exclude = null) where T : class, INPC
    {
        if (typeof(T) == typeof(Villager))
        {
            Villager excludeVillager = exclude as Villager;
            return GetAnyNearbyVillager(position, range, excludeVillager) as T;
        }

        // No more types
        return null;
    }
    #endregion

    #region PRIVATE METHODS
    public Villager GetAnyNearbyVillager(Vector3 position, float range, Villager exclude = null)
    {
        if (_physicsQueryReady)
        {
            try
            {
                int mask = _villagerLayer.value;
                int hits = Physics.OverlapSphereNonAlloc(position, range, _overlapResults, mask, QueryTriggerInteraction.Collide);
                float sqrRange = range * range;
                for (int i = 0; i < hits; i++)
                {
                    var collider = _overlapResults[i];
                    if (collider == null) continue;

                    if (_colliderToVillager == null || !_colliderToVillager.TryGetValue(collider, out Villager villager))
                    {
                        villager = collider.GetComponentInParent<Villager>();
                        if (villager != null && _colliderToVillager != null)
                            try { _colliderToVillager[collider] = villager; } catch { }
                    }

                    if (villager == null) continue;
                    if (villager == exclude) continue;
                    if (!villager.GO.activeInHierarchy) continue;
                    if ((villager.GO.transform.position - position).sqrMagnitude > sqrRange) continue;

                    return villager;
                }
                return null;
            }
            catch { }
        }

        // Fallback: iterate active villager list
        float sqrRangeFallback = range * range;
        foreach (var villager in _activeVillagers)
        {
            if (villager == null) continue;
            if (villager == exclude) continue;
            if (!villager.gameObject.activeInHierarchy) continue;
            if ((villager.transform.position - position).sqrMagnitude <= sqrRangeFallback)
                return villager;
        }
        return null;
    }
    #endregion

    #region EVENT HANDLERS
    // On each milestone change
    void OnTownPopulationChanged(int newPopulation)
    {
        //Debug.Log($"[NPCPoolManager]: Town population changed to {newPopulation}");
        _maxActiveVillagers = Mathf.RoundToInt(newPopulation * _activeVillagersRatio);
    }
    #endregion

    #region POOL METHODS
    /// <summary>
    /// Creation method for villagers pool: instantiates a random villager prefab.
    /// </summary>
    /// <returns>Instantiated villager.</returns>
    Villager CreateVillager()
    {
        // Decide gender for this spawn to pick the correct prefab list
        bool isFemale = UnityEngine.Random.value < _femaleRatio;

        GameObject[] sourceArray = isFemale ?
            _femaleVillagerPrefabs :
            _maleVillagerPrefabs;

        GameObject prefab = sourceArray[UnityEngine.Random.Range(0, sourceArray.Length)];

        Villager villager = Instantiate(
            prefab,
            transform.position,
            Quaternion.identity,
            transform
            ).GetComponent<Villager>();

        string given = _namesDatabase.GetRandomGiven(villager.IsFemale);
        string family = _namesDatabase.GetRandomFamily();
        villager.SetFullName(given, family);

        villager.gameObject.SetActive(false);

        // Register all colliders found in this villager's hierarchy to the collider->villager map
        try
        {
            var cols = villager.GetComponentsInChildren<Collider>(true);
            foreach (var c in cols)
            {
                if (c == null) continue;
                if (!_colliderToVillager.ContainsKey(c))
                    _colliderToVillager[c] = villager;
            }
        }
        catch { }

        return villager;
    }

    void GetVillager(Villager villager)
    {
        House randomFreeHouse = _townManager.GetHouse();
        if (randomFreeHouse == null)
        {
            Debug.LogError("NPCPoolManager.GetVillager: No houses available to assign to new villager.");
            _pendingRelease.Enqueue(villager); // Return to pool next frame — avoids reentrancy with ObjectPool
            return;
        }
        villager.AssignHome(randomFreeHouse);

        Workplace randomWorkplace = _townManager.GetWorkplace();
        if (randomWorkplace != null) // Can be null
            villager.AssignWorkplace(randomWorkplace);

        Sanctuary nearestSanctuary = _townManager.GetNearestSanctuary(randomFreeHouse);
        if (nearestSanctuary == null)
        {
            Debug.LogError("NPCPoolManager.GetVillager: No sanctuaries available to assign to new villager.");
            _pendingRelease.Enqueue(villager);
            return;
        }
        villager.AssignSanctuary(nearestSanctuary);

        Market randomMarket = _townManager.GetNearestMarket(randomFreeHouse);
        if (randomMarket == null)
        {
            Debug.LogError("NPCPoolManager.GetVillager: No markets available to assign to new villager.");
            _pendingRelease.Enqueue(villager);
            return;
        }
        villager.AssignMarket(randomMarket);

        // Activate and reset components
        villager.gameObject.SetActive(true);
        villager.CharacterModel.SetActive(false);
        villager.MovementController.Reset();
        villager.InteractionController.Reset();
        villager.AnimationController.Reset();
        villager.BehaviourSystem ??= villager.DefineBehaviourSystem();

        _activeVillagers.Add(villager);
    }
    #endregion
}
