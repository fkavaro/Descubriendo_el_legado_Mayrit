using System;
using System.Collections.Generic;
using UnityEngine;

public class Stall : Workplace
{
    #region EDITOR PROPERTIES
    [Header("Stall Properties")]
    [SerializeField] Market _parentMarket;
    #endregion

    #region LIFE CYCLE
    public override void OnEnable()
    {
        base.OnEnable();

        _isInterior = false;

        // Get parent with market component (searches this GameObject and ancestors)
        if (_parentMarket == null)
            _parentMarket = GetComponentInParent<Market>();

        if (_parentMarket != null)
            _parentMarket.RegisterStall(this);
    }

    public override void OnDisable()
    {
        base.OnDisable();

        // Unregister from parent market if previously registered
        if (_parentMarket != null)
        {
            try { _parentMarket.UnregisterStall(this); } catch { }
        }
    }
    #endregion
}
