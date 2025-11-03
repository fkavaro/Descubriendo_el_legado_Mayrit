using System.Collections.Generic;
using UnityEngine;

public abstract class ABuilding : MonoBehaviour
{
    [SerializeField] List<Spot> _entranceSpots;

    public void PlaceAtRandomEntrance(Villager villager) // TODO INPC instead of Villager
    {
        Spot entranceSpot = GetRandomEntranceSpot();
        if (entranceSpot != null)
        {
            villager.transform.position = entranceSpot.transform.position;
            villager.ForceRotation(entranceSpot.DirectionVector);
        }
        else
        {
            villager.transform.position = transform.position;
            Debug.LogWarning($"No entrance spots defined for building {gameObject.name}. Placing villager at building position.");
        }
    }

    public Spot GetRandomEntranceSpot()
    {
        if (_entranceSpots.Count == 0) return null;
        int randomIndex = Random.Range(0, _entranceSpots.Count);
        return _entranceSpots[randomIndex];
    }
}
