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

    public Spot GetRandomStallSpot()
    {
        Stall stall = GetRandomStall();
        if (stall == null) return null;
        return stall.GetRandomAccessSpot();
    }
    #endregion
}
