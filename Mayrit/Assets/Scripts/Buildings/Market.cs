using System;
using System.Collections.Generic;
using UnityEngine;

public class Market : ABuilding
{
    #region EDITOR PROPERTIES
    [Header("Market Properties")]
    [SerializeField] int _numberOfStalls;
    readonly HashSet<Stall> _stalls = new();
    #endregion

    #region INHERITED METHODS
    public override void RegisterBuilding()
    {
        TownManager.RegisterMarket(this);
    }

    public override void UnregisterBuilding()
    {
        TownManager.UnregisterMarket(this);
    }
    #endregion

    #region PUBLIC METHODS
    public void RegisterStall(Stall stall)
    {
        if (stall == null) return;
        _stalls.Add(stall);
        _numberOfStalls = _stalls.Count;
    }

    public void UnregisterStall(Stall stall)
    {
        if (stall == null) return;
        _stalls.Remove(stall);
        _numberOfStalls = _stalls.Count;
    }

    public Stall GetRandomOpenedStall()
    {
        if (_stalls.Count == 0) return null;

        List<Stall> openedStalls = new();

        foreach (Stall stall in _stalls)
        {
            if (stall != null && stall._isOpen)
                openedStalls.Add(stall);
        }

        if (openedStalls.Count == 0) return null;

        int randomIndex = UnityEngine.Random.Range(0, openedStalls.Count);
        return openedStalls[randomIndex];
    }
    #endregion
}
