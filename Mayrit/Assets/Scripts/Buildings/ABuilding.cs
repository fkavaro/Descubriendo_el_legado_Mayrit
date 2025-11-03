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
    }

    public Spot GetRandomEntranceSpot()
    {
        if (_entranceSpots.Count == 0) return null;
        int randomIndex = Random.Range(0, _entranceSpots.Count);
        return _entranceSpots[randomIndex];
    }
}
