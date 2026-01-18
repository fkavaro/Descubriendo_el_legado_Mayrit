using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
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
    #endregion

    #region INTERNAL PROPERTIES
    public ObjectPool<Villager> _villagerPool;

    Collider[] _overlapResults; // Cached buffer for OverlapSphereNonAlloc
    Dictionary<Collider, Villager> _colliderToVillager; // Cache to avoid GetComponent calls on colliders returned by physics queries

    // Dependency Injection
    TownManager _townManager;
    #endregion

    #region LIFE CYCLE
    void Awake()
    {
        // Pool creation
        _villagerPool = new ObjectPool<Villager>(
            createFunc: CreateVillager,
            actionOnGet: GetVillager,
            actionOnRelease: ReleaseVillager,
            actionOnDestroy: (villager) => Destroy(villager.gameObject)
        );

        // Allocate cached overlap buffer
        _overlapResults = new Collider[_maxOverlapResults];
        // Initialize collider->villager cache
        _colliderToVillager = new Dictionary<Collider, Villager>(_maxActiveVillagers > 0 ? _maxActiveVillagers * 2 : 32);
    }

    void Start()
    {
        // Get dependencies from ServiceLocator
        _townManager = ServiceLocator.Instance.Get<TownManager>();

        // Subscribe to town population changes
        _townManager.OnPopulationChanged += OnTownPopulationChanged;
    }

    void Update()
    {
        // Spawn a villager (per frame) if active villagers are below max
        if (_villagerPool != null && _activeVillagers.Count < _maxActiveVillagers)
            _villagerPool.Get();
    }

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

    void OnDestroy()
    {
        // Unsubscribe from town population changes (guarded to avoid NullReferenceException during teardown)
        _townManager.OnPopulationChanged -= OnTownPopulationChanged;
    }
    #endregion

    #region PUBLIC METHODS
    /// <summary>
    /// Public method to return a villager instance back to the pool.
    /// </summary>
    public void ReturnVillagerToPool(Villager villager)
    {
        if (_villagerPool == null || villager == null) return;
        _villagerPool.Release(villager);
    }

    /// <summary>
    /// Returns a list of active villagers within the specified range from a position.
    /// The optional exclude parameter can be used to ignore a specific villager (usually the caller).
    /// </summary>
    public List<Villager> GetNearbyVillagers(Vector3 position, float range, Villager exclude = null)
    {
        var nearbyVillagers = new List<Villager>();

        // Prefer physics-based broadphase if a layer mask and overlap buffer are available.
        try
        {
            if (_overlapResults != null && _overlapResults.Length > 0 && _villagerLayer != 0)
            {
                int mask = _villagerLayer.value;
                int hits = Physics.OverlapSphereNonAlloc(position, range, _overlapResults, mask, QueryTriggerInteraction.Collide);
                for (int i = 0; i < hits; i++)
                {
                    var collider = _overlapResults[i];
                    if (collider == null) continue;

                    if (_colliderToVillager != null && _colliderToVillager.TryGetValue(collider, out Villager villager))
                    {
                        // got cached villager
                    }
                    else
                    {
                        // fallback: try to resolve and cache
                        villager = collider.GetComponentInParent<Villager>();
                        if (villager != null && _colliderToVillager != null)
                        {
                            try { _colliderToVillager[collider] = villager; } catch { }
                        }
                    }

                    if (villager == null) continue;
                    if (villager == exclude) continue;
                    if (!villager.GO.activeInHierarchy) continue;

                    // check exact distance to be safe
                    if ((villager.GO.transform.position - position).sqrMagnitude <= range * range)
                        nearbyVillagers.Add(villager);
                }

                return nearbyVillagers;
            }
        }
        catch { /* fall back to managed list below on error */ }

        // Fallback: iterate managed active villager list
        if (_activeVillagers == null || _activeVillagers.Count == 0) return nearbyVillagers;
        float sqrRange = range * range;
        foreach (var villager in _activeVillagers)
        {
            if (villager == null) continue;
            if (villager == exclude) continue;
            if (!villager.gameObject.activeInHierarchy) continue;

            if ((villager.transform.position - position).sqrMagnitude <= sqrRange)
                nearbyVillagers.Add(villager);
        }

        return nearbyVillagers;
    }

    /// <summary>
    /// GC-free: returns the first nearby Villager found within range (or null).
    /// Useful when you only need one target (avoids allocating a results list).
    /// </summary>
    public Villager GetAnyNearbyVillager(Vector3 position, float range, Villager exclude = null)
    {
        // Reuse existing GetNearbyVillagers implementation to avoid duplicating physics/cache logic.
        var list = GetNearbyVillagers(position, range, exclude);
        if (list != null && list.Count > 0) return list[0];
        return null;
    }

    /// <summary>
    /// GC-free boolean check: returns true if any villager is nearby (uses GetAnyNearbyVillager under the hood).
    /// </summary>
    public bool IsAnyVillagerNearby(Vector3 position, float range, Villager exclude = null)
    {
        return GetAnyNearbyVillager(position, range, exclude) != null;
    }

    /// <summary>
    /// Generic version: returns the first nearby NPC of type T within range (or null).
    /// Filters results to only return NPCs of the specified type.
    /// </summary>
    public T GetAnyNearby<T>(Vector3 position, float range, INPC exclude = null) where T : class, INPC
    {
        // For Villager type, use the optimized Villager-specific method
        if (typeof(T) == typeof(Villager))
        {
            Villager excludeVillager = exclude as Villager;
            return GetAnyNearbyVillager(position, range, excludeVillager) as T;
        }

        // Generic fallback: check all active villagers and filter by type
        var list = GetNearbyVillagers(position, range, exclude as Villager);
        if (list != null && list.Count > 0)
        {
            foreach (var npc in list)
            {
                if (npc is T typedNpc)
                    return typedNpc;
            }
        }
        return null;
    }
    #endregion

    #region PRIVATE METHODS
    /// <summary>
    /// Handle town population change events: spawn or retire villagers proportionally to population
    /// </summary>
    void OnTownPopulationChanged(int newPopulation)
    {
        // Update target active villager count. Spawning (growth) is handled
        // incrementally by Update() to avoid hitches — it will add at most
        // one villager per frame until the target is reached.
        _maxActiveVillagers = Mathf.RoundToInt(newPopulation * _activeVillagersRatio);

        // If we need to retire villagers (active > max), release extras immediately.
        int currentActive = _activeVillagers.Count;
        int activeDifference = _maxActiveVillagers - currentActive;
        if (activeDifference < 0)
        {
            int toRetire = -activeDifference;
            for (int i = 0; i < toRetire; i++)
            {
                if (_activeVillagers.Count == 0) break;

                Villager lastActiveVillager = _activeVillagers[^1];
                ReturnVillagerToPool(lastActiveVillager);
            }
        }
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
        // Track active
        if (!_activeVillagers.Contains(villager))
            _activeVillagers.Add(villager);

        // Pool assigns a name accordingly to its gender
        try
        {
            string given = _namesDatabase.GetRandomGiven(villager.IsFemale);
            string family = _namesDatabase.GetRandomFamily();
            villager.SetFullName(given, family);
        }
        catch { }

        House randomFreeHouse = _townManager.GetHouse();
        villager.AssignHome(randomFreeHouse);

        Workplace randomWorkplace = _townManager.GetWorkplace();
        villager.AssignWorkplace(randomWorkplace);

        Sanctuary nearestSanctuary = _townManager.GetNearestSanctuary(randomFreeHouse);
        villager.AssignSanctuary(nearestSanctuary);

        Market randomMarket = _townManager.GetNearestMarket(randomFreeHouse);
        villager.AssignMarket(randomMarket);

        // Activate and reset components
        randomFreeHouse.PlaceAtRandomAccess(villager);
        villager.BehaviourSystem.Reset();
        villager.MovementController.Reset();
        villager.InteractionController.Reset();
        villager.AnimationController.Reset();
        villager.gameObject.SetActive(true);
        villager.Agent.enabled = true; // Activated once its placed
    }

    /// <summary>
    /// Release method for villagers pool: deactivates villager gameobject.
    /// </summary>
    void ReleaseVillager(Villager villager)
    {
        // Let the villager clear references/state before deactivation
        try { villager.OnReleasedFromPool(); } catch { }

        // Track active
        if (_activeVillagers.Contains(villager))
            _activeVillagers.Remove(villager);
    }
    #endregion
}
