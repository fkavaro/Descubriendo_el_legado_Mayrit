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
    [Tooltip("Maximum number of villager at once")]
    public int _maxActiveVillagers = 10;
    public List<Villager> _activeVillagers = new();

    // [Tooltip("How many villager spawns to perform per frame when batching")]
    // public int _spawnPerFrame = 3;
    // [Tooltip("How many prefab instantiations to perform per frame when prewarming the pool")]
    // public int _prewarmPerFrame = 5;
    // [Tooltip("Optional spawn points to use when TownManager requests global spawns")]
    // public Transform[] _globalSpawnPoints;
    #endregion

    #region PRIVATE PROPERTIES

    #endregion

    #region MONOBEHAVIOUR
    void Update()
    {
        var tm = TownManager.ExistingInstance;
        if (tm == null || tm._population <= 0)
            return;

        // Active villagers not at max
        if (_activeVillagers.Count < _maxActiveVillagers)
        {
            // Spawn a new villager
            _villagerPool.Get();
        }
    }

    void OnEnable()
    {
        // Pool creation
        _villagerPool = new ObjectPool<Villager>(
            createFunc: CreateVillager,
            actionOnGet: GetVillager,
            actionOnRelease: ReleaseVillager,
            actionOnDestroy: (villager) => Destroy(villager.gameObject)
            //,maxSize: _maxActiveVillagers
        );

        // Subscribe to town population changes
        TownManager.Instance.OnPopulationChanged += OnTownPopulationChanged;
    }

    private void OnTownPopulationChanged(int population)
    {
        // Max active players is a third of population
        _maxActiveVillagers = population / 3; // TODO revisit
    }

    void OnDestroy()
    {
        // Unsubscribe from town population changes
        TownManager.Instance.OnPopulationChanged -= OnTownPopulationChanged;
    }
    #endregion

    #region PUBLIC METHODS
    // public void SpawnAndAssignNewResident(House house)
    // {
    //     Villager newVillager = SpawnVillagerAtRandomSpot(house);

    // }

    // /// <summary>
    // /// Spawn multiple villagers for a building, spread across frames to avoid spikes.
    // /// </summary>
    // public void SpawnVillagersForHouseBatched(ABuilding building, int count)
    // {
    //     if (count <= 0 || building == null) return;
    //     StartCoroutine(SpawnVillagersForHouseCoroutine(building, count));
    // }

    /// <summary>
    /// Public method to return a villager instance back to the pool.
    /// </summary>
    public void ReturnVillagerToPool(Villager villager)
    {
        if (_villagerPool == null || villager == null) return;
        _villagerPool.Release(villager);
    }

    // /// <summary>
    // /// Prewarm the pool by instantiating `amount` villagers and immediately releasing them back to the pool.
    // /// This is performed across frames to avoid hitches.
    // /// </summary>
    // public void PrewarmPool(int amount)
    // {
    //     if (amount <= 0 || _villagerPool == null) return;
    //     StartCoroutine(PrewarmPoolCoroutine(amount));
    // }
    #endregion

    #region PRIVATE METHODS
    // Villager SpawnVillagerAtRandomSpot()
    // {
    //     if (_villagerPrefabs == null || _villagerPrefabs.Length == 0) return null;
    //     if (_villagerPool == null) return null;

    //     Villager newVillager = _villagerPool.Get();


    //     return newVillager;
    // }

    // // Handle town population change events: spawn or retire villagers to match desired population
    // void OnTownPopulationChanged(int newPopulation)
    // {
    //     int desired = Mathf.Clamp(newPopulation, 0, _maxActiveVillagers);
    //     int active = _activeVillagers.Count;
    //     int delta = desired - active;

    //     if (delta > 0)
    //     {
    //         // Spawn delta villagers using global spawn points if available
    //         StartCoroutine(SpawnGlobalVillagersCoroutine(delta));
    //     }
    //     else if (delta < 0)
    //     {
    //         int toRetire = -delta;
    //         for (int i = 0; i < toRetire; i++)
    //         {
    //             if (_activeVillagers.Count == 0) break;
    //             Villager v = _activeVillagers[_activeVillagers.Count - 1];
    //             // Ask villager to go home and release
    //             try { v.ReturnHomeAndRelease(); } catch { }
    //         }
    //     }
    // }
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

        // Use ExistingInstance so a TownManager is not created during teardown
        House randomFreeHouse = TownManager.ExistingInstance.GetRandomHouseWithFreeSpace();
        Building randomWorkplace = TownManager.ExistingInstance.GetRandomWorkplaceBuilding();

        // Assign home and workplace
        villager.AssignHome(randomFreeHouse);
        randomFreeHouse.AssignNewResident(villager);
        villager.AssignWorkplace(randomWorkplace);

        // Activate and reset components
        villager.gameObject.SetActive(true);
        villager.BehaviourSystem.Reset(); // TODO this won't reset work place. InitializeBehaviourSystem should be executed again to do that
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
    }


    #endregion

    #region COROUTINES
    // IEnumerator SpawnVillagersForHouseCoroutine(ABuilding building, int count)
    // {
    //     int spawned = 0;
    //     while (spawned < count)
    //     {
    //         int batch = Mathf.Min(_spawnPerFrame, count - spawned);
    //         for (int i = 0; i < batch; i++)
    //         {
    //             SpawnVillagerAtRandomSpot(building);
    //             spawned++;
    //         }
    //         // yield a frame to spread cost
    //         yield return null;
    //     }
    // }

    // IEnumerator PrewarmPoolCoroutine(int amount)
    // {
    //     int created = 0;
    //     while (created < amount)
    //     {
    //         int batch = Mathf.Min(_prewarmPerFrame, amount - created);
    //         for (int i = 0; i < batch; i++)
    //         {
    //             Villager v = _villagerPool.Get();
    //             _villagerPool.Release(v);
    //             created++;
    //         }
    //         yield return null;
    //     }
    // }

    // IEnumerator SpawnGlobalVillagersCoroutine(int amount)
    // {
    //     int spawned = 0;
    //     while (spawned < amount)
    //     {
    //         int batch = Mathf.Min(_spawnPerFrame, amount - spawned);
    //         for (int i = 0; i < batch; i++)
    //         {
    //             Villager v = _villagerPool.Get();
    //             // Place at a random global spawn point if available
    //             if (_globalSpawnPoints != null && _globalSpawnPoints.Length > 0)
    //             {
    //                 Transform s = _globalSpawnPoints[UnityEngine.Random.Range(0, _globalSpawnPoints.Length)];
    //                 v.transform.position = s.position;
    //                 v.ForceRotation(s.rotation);
    //             }
    //             else
    //             {
    //                 v.transform.position = transform.position;
    //             }
    //             spawned++;
    //         }
    //         yield return null;
    //     }
    // }
    #endregion
}
