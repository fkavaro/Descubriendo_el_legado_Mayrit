using System;
using System.Collections.Generic;
using UnityEngine;

public class Stall : Workplace
{
    #region EDITOR PROPERTIES
    [Header("Stall Properties")]
    [SerializeField] Market _parentMarket;
    [SerializeField] int _maxClientsWaiting = 3;
    [SerializeField] int _currentClientsWaiting;
    readonly HashSet<INPC> _clientsWaiting = new();

    public bool TooManyClientsWaiting => _clientsWaiting.Count >= _maxClientsWaiting;
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
            _parentMarket.UnregisterStall(this);
    }
    #endregion

    #region PUBLIC METHODS
    public void RegisterClientWaiting(INPC npc)
    {
        if (npc == null) return;
        _clientsWaiting.Add(npc);
        _currentClientsWaiting = _clientsWaiting.Count;
    }

    public void UnregisterClientWaiting(INPC npc)
    {
        if (npc == null) return;
        _clientsWaiting.Remove(npc);
        _currentClientsWaiting = _clientsWaiting.Count;
    }
    #endregion
}
