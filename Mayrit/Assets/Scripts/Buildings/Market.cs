using System;
using System.Collections.Generic;
using UnityEngine;

public class Market : ABuilding
{
    #region EDITOR PROPERTIES
    [SerializeField] List<Stall> _stalls = new();
    #endregion

    #region PUBLIC METHODS
    public void RegisterStall(Stall stall)
    {
        if (stall == null) return;
        if (!_stalls.Contains(stall))
            _stalls.Add(stall);
    }

    public void UnregisterStall(Stall stall)
    {
        if (stall == null) return;
        if (_stalls.Contains(stall))
            _stalls.Remove(stall);
    }

    public Stall GetRandomStall()
    {
        if (_stalls == null || _stalls.Count == 0) return null;
        int randomIndex = UnityEngine.Random.Range(0, _stalls.Count);
        return _stalls[randomIndex];
    }


    /// <returns>A random opened stall from the market</returns>
    public Stall GetRandomOpenedStall()
    {
        if (_stalls == null || _stalls.Count == 0) return null;

        List<Stall> openedStalls = new();

        foreach (var stall in _stalls)
        {
            if (stall.IsOpen())
                openedStalls.Add(stall);
        }

        if (openedStalls.Count == 0) return null;

        int randomIndex = UnityEngine.Random.Range(0, openedStalls.Count);
        return openedStalls[randomIndex];
    }

    /// <returns>The access spot of a random opened stall in the market
    public Spot GetRandomStallSpot()
    {
        Spot spot = null;
        Stall stall = GetRandomStall();

        if (stall != null)
            spot = stall.GetRandomAccessSpot();

        return spot;
    }

    public bool IsSomeoneWorking()
    {
        foreach (var stall in _stalls)
        {
            if (stall.IsOpen())
                return true;
        }
        return false;
    }
    #endregion
}
