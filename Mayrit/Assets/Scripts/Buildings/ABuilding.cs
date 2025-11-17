using System.Collections.Generic;
using UnityEngine;

public abstract class ABuilding : MonoBehaviour
{
    #region EDITOR PROPERTIES
    [Header("Building Properties")]
    [SerializeField] List<Spot> _accessSpots;
    #endregion

    #region ABSTRACT METHODS
    public abstract void RegisterBuilding();
    public abstract void UnregisterBuilding();
    #endregion

    #region MONOBEHAVIOUR
    public virtual void OnEnable()
    {
        RegisterBuilding();
    }

    public virtual void OnDisable()
    {
        UnregisterBuilding();
    }
    #endregion

    #region PUBLIC METHODS
    public void PlaceAtRandomEntrance(INPC npc)
    {
        if (npc == null || npc.Agent == null)
        {
            Debug.LogWarning($"PlaceAtRandomEntrance: npc or npc.Agent is null for building {gameObject.name}.");
            return;
        }

        Spot entranceSpot = GetRandomAccessSpot();
        if (entranceSpot != null)
        {
            npc.Agent.transform.position = entranceSpot.transform.position;
            npc.ForceRotation(entranceSpot.WorldDirection);
        }
        else
        {
            npc.Agent.transform.position = transform.position;
            Debug.LogWarning($"No entrance spots defined for building {gameObject.name}. Placing villager at building position.");
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
