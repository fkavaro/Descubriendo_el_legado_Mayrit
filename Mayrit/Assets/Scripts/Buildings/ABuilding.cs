using System.Collections.Generic;
using UnityEngine;

public abstract class ABuilding : MonoBehaviour
{
    public List<Spot> AccessSpots => _accessSpots;

    #region EDITOR PROPERTIES
    [Header("Building Properties")]
    [SerializeField] List<Spot> _accessSpots;
    #endregion

    #region INTERNAL PROPERTIES
    TownManager _townManager;
    protected TownManager TownManager => _townManager;
    #endregion

    #region ABSTRACT METHODS
    public abstract void RegisterBuilding();
    public abstract void UnregisterBuilding();
    #endregion

    #region LIFE CYCLE
    public virtual void OnEnable()
    {
        // Get from ServiceLocator
        _townManager = ServiceLocator.Instance.Get<TownManager>();

        if (_townManager == null)
        {
            Debug.LogError($"[{gameObject.name}.OnEnable] TownManager not found.", gameObject);
            return;
        }

        RegisterBuilding();
    }

    public virtual void OnDisable()
    {
        UnregisterBuilding();
    }
    #endregion

    #region PUBLIC METHODS
    public void PlaceAtRandomAccess(INPC npc)
    {
        if (npc == null || npc.Agent == null)
        {
            Debug.LogWarning($"[{gameObject.name}PlaceAtRandomAccess] Npc or npc.Agent is null.", gameObject);
            return;
        }

        Spot accessSpot = GetRandomAccessSpot();
        if (accessSpot != null)
        {
            npc.MovementController.PlaceAt(accessSpot.transform.position);
            npc.MovementController.ForceRotation(accessSpot.WorldDirection);
        }
        else
        {
            npc.MovementController.PlaceAt(transform.position);
            Debug.LogWarning($"[{gameObject.name}PlaceAtRandomAccess] No entrance spots defined. Placing npc at building position.");
        }
    }

    public Spot GetRandomAccessSpot()
    {
        if (_accessSpots == null || _accessSpots.Count == 0) return null;
        int randomIndex = Random.Range(0, _accessSpots.Count);
        return _accessSpots[randomIndex];
    }
    #endregion
}
