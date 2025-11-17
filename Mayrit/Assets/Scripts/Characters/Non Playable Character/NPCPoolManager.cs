using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Pool manager for NPCs (Non-Player Characters).
/// </summary>
public class NPCPoolManager : Singleton<NPCPoolManager>
{
    #region PUBLIC PROPERTIES
    public ObjectPool<Villager> _villagerPool;
    // TODO: soldier pool

    [Header("Villagers pool")]
    [Tooltip("All villager models to be spawned randomly")]
    public GameObject[] _villagerPrefabs;
    [Tooltip("Ratio of active villagers to total population"), Range(0f, 1f)]
    public float _activeVillagersRatio = 0.3f;
    [Tooltip("Maximum number of villager at once")]
    public int _maxActiveVillagers;
    public List<Villager> _activeVillagers = new();
    #endregion

    #region PRIVATE PROPERTIES

    #endregion

    #region MONOBEHAVIOUR
    void OnEnable()
    {
        // Pool creation
        _villagerPool = new ObjectPool<Villager>(
            createFunc: CreateVillager,
            actionOnGet: GetVillager,
            actionOnRelease: ReleaseVillager,
            actionOnDestroy: (villager) => Destroy(villager.gameObject)
        );

        // Subscribe to town population changes
        TownManager.Instance.OnPopulationChanged += OnTownPopulationChanged;
    }

    void OnDestroy()
    {
        // Unsubscribe from town population changes (guarded to avoid NullReferenceException during teardown)
        try
        {
            var tm = TownManager.ExistingInstance;
            if (tm != null)
                tm.OnPopulationChanged -= OnTownPopulationChanged;
        }
        catch { }
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
    #endregion

    #region PRIVATE METHODS
    /// <summary>
    /// Handle town population change events: spawn or retire villagers proportionally to population
    /// </summary>
    void OnTownPopulationChanged(int newPopulation)
    {
        _maxActiveVillagers = Mathf.RoundToInt(newPopulation * _activeVillagersRatio);

        int currentActive = _activeVillagers.Count;
        int activeDifference = _maxActiveVillagers - currentActive;

        // More active villagers are desired
        if (activeDifference > 0)
        {
            // Spawn needed villagers
            for (int i = 0; i < activeDifference; i++)
                _villagerPool.Get();
        }
        // Fewer active villagers are desired
        else if (activeDifference < 0)
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
        GameObject prefab = _villagerPrefabs[UnityEngine.Random.Range(0, _villagerPrefabs.Length)];

        Villager villager = Instantiate(
            prefab,
            transform.position,
            Quaternion.identity,
            transform
            ).GetComponent<Villager>();

        villager.gameObject.SetActive(false);

        return villager;
    }

    void GetVillager(Villager villager)
    {
        // Track active
        if (!_activeVillagers.Contains(villager))
            _activeVillagers.Add(villager);

        House randomFreeHouse = TownManager.Instance.GetHouse();
        villager.AssignHome(randomFreeHouse);

        Workplace randomWorkplace = TownManager.Instance.GetWorkplaceWithFreeCapacity();
        villager.AssignWorkplace(randomWorkplace);

        Sanctuary nearestSanctuary = TownManager.Instance.GetNearestSanctuary(randomFreeHouse);
        villager.AssignSanctuary(nearestSanctuary);

        Market randomMarket = TownManager.Instance.GetNearestMarket(randomFreeHouse);
        villager.AssignMarket(randomMarket);

        // Activate and reset components
        villager.gameObject.SetActive(true);
        villager.InitializeBehaviourSystem(); // Again
        villager._animationController.ChangeToWalk();
        randomFreeHouse.PlaceAtRandomEntrance(villager);
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

        // Spawn a replacement if active villagers are below max
        try
        {
            if (_villagerPool != null && _activeVillagers.Count < _maxActiveVillagers)
            {
                _villagerPool.Get();
            }
        }
        catch { }
    }
    #endregion
}
