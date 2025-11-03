using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class TownManager : Singleton<TownManager>
{
    #region EDITOR PROPERTIES
    [Header("Town Stats")]
    public int _population;
    #endregion

    #region INTERNAL PROPERTIES    
    /// <summary>
    /// Event fired when population changes. Provides the new population value.
    /// </summary>
    public event Action<int> OnPopulationChanged;
    readonly List<House> _houses = new();
    #endregion

    #region PUBLIC METHODS  
    /// <summary>
    /// Registers a house in the town and updates population accordingly.
    /// </summary>
    public void RegisterHouse(House house)
    {
        if (house == null) return;

        if (!_houses.Contains(house))
        {
            _houses.Add(house);
            UpdatePopulation(house._householdSize);
        }
    }

    /// <summary>
    /// Unregisters a house from the town and updates population accordingly.
    /// </summary>
    public void UnregisterHouse(House house)
    {
        if (house == null) return;

        if (_houses.Contains(house))
        {
            _houses.Remove(house);
            UpdatePopulation(-house._householdSize);
        }
    }


    /// <returns>Random registered house with capacity for a new resident.</returns>
    public House GetRandomHouseWithFreeSpace()
    {
        if (_houses == null || _houses.Count == 0) return null;

        // Build a list of candidate houses with available slots
        List<House> housesWithFreeSlots = new();

        // Check every house
        foreach (var house in _houses)
        {
            if (house.HasCapacityForNewResident())
                housesWithFreeSlots.Add(house);
        }

        // No houses with free slots found
        if (housesWithFreeSlots.Count == 0)
            return null;

        // Return a random house from the candidates
        return housesWithFreeSlots[UnityEngine.Random.Range(0, housesWithFreeSlots.Count)];
    }

    #region PRIVATE METHODS
    void UpdatePopulation(int householdSize)
    {
        _population += householdSize;
        OnPopulationChanged?.Invoke(_population);
    }
    #endregion

    /// <summary>
    /// Attempts to reassign residents from a destroyed house to other houses with free capacity.
    /// If a resident cannot be reassigned, it will be returned to the NPC pool and population decremented.
    /// </summary>
    public void ReassignResidents(House fromHouse, List<Villager> residents)
    {
        if (residents == null || residents.Count == 0) return;

        // Build candidate list: all registered houses that have at least one free slot.
        // Exclude the source (destroyed) house so we don't reassign back to it.
        var candidates = new List<House>();
        for (int i = 0; i < _houses.Count; i++)
        {
            var h = _houses[i];
            if (h == null || h == fromHouse) continue;
            if (h.HasCapacityForNewResident())
                candidates.Add(h);
        }

        int releasedCount = 0;

        // Shortcut to the NPC pool manager to return villagers we cannot place.
        var pool = NPCPoolManager.Instance;

        // Process each villager independently. We handle exceptions per-villager so a single
        // failure does not abort reassignment of others.
        for (int i = 0; i < residents.Count; i++)
        {
            var v = residents[i];
            if (v == null) continue;

            try
            {
                // If there are no candidate houses left, we must release the villager back to the pool.
                // This decrements town population later (below) for all released villagers.
                if (candidates.Count == 0)
                {
                    pool?.ReturnVillagerToPool(v);
                    releasedCount++;
                }

                // Choose a candidate at random to distribute load across houses.
                int idx = UnityEngine.Random.Range(0, candidates.Count);
                var best = candidates[idx];

                // Defensive check: if the selected house is unexpectedly null, release the villager.
                if (best == null)
                {
                    pool?.ReturnVillagerToPool(v);
                    releasedCount++;
                }

                // Perform the reassignment:
                // - Update the villager's home reference so house/resident lists stay consistent.
                // - Move the villager to an entrance/spawn spot if available (for visual correctness).
                // - Optionally fix rotation if the spawn spot requires it.
                v.AssignHome(best);

                // var spawnSpot = best.GetRandomEntranceSpot();
                // if (spawnSpot != null)
                // {
                //     // Place the villager at the spawn spot and apply rotation if the spot enforces it.
                //     v.transform.position = spawnSpot.transform.position;
                //     if (spawnSpot._isRotationFixed)
                //         v.ForceRotation(spawnSpot.DirectionVector);
                // }
                // else
                // {
                //     // Fallback: place at the house origin if no spawn spot exists.
                //     v.transform.position = best.transform.position;
                // }


                // // If this house reached capacity after the assignment, remove it from the candidate pool
                // // so subsequent villagers aren't assigned to an already-full house.
                // if (best._residents.Count >= best._householdSize)
                //     candidates.RemoveAt(idx);
            }
            catch (Exception ex)
            {
                // Log the error for diagnostics and make a best-effort attempt to release the villager
                // so we don't leak a simulation entity into an invalid state.
                Debug.LogError($"ReassignResidents: exception while reassigning {v?.name}: {ex}");
                try { pool?.ReturnVillagerToPool(v); } catch { /* swallow secondary errors */ }
                releasedCount++;
            }
        }

        // If any villagers were released to the pool (i.e., lost homes), decrement the town population.
        if (releasedCount > 0)
            UpdatePopulation(-releasedCount);
    }

    // /// <summary>
    // /// Returns a random registered house excluding the given one (or null if none available).
    // /// </summary>
    // public House GetRandomHouseExcept(House exclude)
    // {
    //     if (_houses == null || _houses.Count == 0) return null;
    //     if (_houses.Count == 1 && _houses.Contains(exclude)) return null;

    //     int count = _houses.Count;
    //     if (count == 1)
    //         return _houses[0] == exclude ? null : _houses[0];

    //     // Try up to 'count' times to pick a random house that's not the excluded one.
    //     for (int i = 0; i < count; i++)
    //     {
    //         var candidate = _houses[UnityEngine.Random.Range(0, count)];
    //         if (candidate != exclude) return candidate;
    //     }

    //     // Fallback: no suitable candidate found
    //     return null;
    // }

    // /// <summary>
    // /// Returns a random registered house that has free capacity (residents < householdSize),
    // /// excluding the provided house. Returns null if none available.
    // /// </summary>
    // public House GetRandomHouseWithFreeSlotExcept(House exclude)
    // {
    //     if (_houses == null || _houses.Count == 0) return null;

    //     // Build a list of candidate houses with available slots
    //     var candidates = new List<House>();
    //     for (int i = 0; i < _houses.Count; i++)
    //     {
    //         var h = _houses[i];
    //         if (h == null || h == exclude) continue;
    //         if (h._residents.Count < h._householdSize)
    //             candidates.Add(h);
    //     }

    //     if (candidates.Count == 0) return null;

    //     return candidates[UnityEngine.Random.Range(0, candidates.Count)];
    // }
    #endregion
}
