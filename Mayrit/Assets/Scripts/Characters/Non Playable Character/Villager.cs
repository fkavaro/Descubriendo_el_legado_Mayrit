using UnityEngine;
using UnityEngine.AI;

public class Villager : ANPC<FiniteStateMachine>
{
    #region EDIROR PROPERTIES
    /// <summary>
    /// Assigned home.
    /// </summary>
    public House _house;
    #endregion

    #region INHERITED
    public override FiniteStateMachine InitializeBehaviourSystem()
    {
        return new FiniteStateMachine(this);
    }
    #endregion

    #region PUBLIC METHODS
    public void AssignHome(House home)
    {
        _house = home;
    }

    public void OnReleasedFromPool()
    {
        _house.RemoveResident(this);
        _house = null;
    }

    public void ReturnHomeAndRelease()
    {
        // // Optionally move to a home entrance spot before release
        // if (_house != null)
        // {
        //     Spot spawnSpot = _house.GetRandomEntranceSpot();
        //     if (spawnSpot != null)
        //     {
        //         transform.position = spawnSpot.transform.position;
        //         if (spawnSpot._isRotationFixed)
        //             ForceRotation(spawnSpot.DirectionVector);
        //     }
        //     else
        //     {
        //         transform.position = _house.transform.position;
        //     }
        // }

        // Return to pool
        NPCPoolManager.Instance.ReturnVillagerToPool(this);
    }
    #endregion
}
