using UnityEngine;
using UnityEngine.AI;

public interface INPC : IBehaviourEntity
{
    #region PUBLIC METHODS
    public NavMeshAgent Agent { get; }
    public AnimationController AnimationController { get; }

    /// <summary>
    /// Sets the destination position for the NavMeshAgent to navigate to
    /// </summary>
    public void SetDestination(Vector3 destinationPos);
    /// <summary>
    /// Sets the destination spot for the NavMeshAgent to navigate to
    /// </summary>
    public void SetDestinationSpot(Spot destinationSpot);

    /// <returns>If the current destination spot is occupied</returns>
    public bool DestinationSpotIsOccupied();

    /// <summary>
    /// Checks if the NavMeshAgent is close to a certain position
    /// and makes it look at it when arriving, if wanted.
    /// </summary>
    public bool IsCloseTo(Vector3 destination, float checkingDistance = 2f, bool lookAtDestination = false);
    /// <summary>
    /// Checks if the NavMeshAgent is close to its destination
    /// and makes it look at it when arriving, if wanted.
    /// </summary>
    public bool IsCloseToDestination(float checkingDistance = 2f, bool lookAtDestination = false);

    /// <summary>
    /// Checks if the NavMeshAgent has arrived at its destination, 
    /// and if the target is a spot, fixes its rotation if wanted.
    /// </summary>
    public bool HasArrivedAtDestination(bool fixRotation = true, bool fixPosition = true);
    /// <summary>
    /// Checks if the NavMeshAgent has arrived at certain spot position
    /// and if the target is a spot, fixes its rotation if wanted.
    /// </summary>
    public bool HasArrived(Spot spot, bool fixRotation = true, bool fixPosition = true);
    /// <summary>
    /// Checks if the NavMeshAgent has arrived at certain destination
    /// and if the target is a spot, fixes its rotation if wanted.
    /// </summary>
    public bool HasArrived(Vector3 destination, bool fixRotation = true, bool fixPosition = true);

    public void ForceRotation(Vector3 lookDirection);
    public void ForceRotation(Quaternion rotation);

    /// <summary>
    /// Checks if the NavMeshAgent can move to the specified position,
    /// taking out its nearest reachable position.
    /// </summary>
    public bool CanReachPosition(Vector3 targetPos, out Vector3 reachablePos);

    /// <summary>
    /// Returns true if a random point is reachable
    /// </summary>
    public bool CalculateRandomDestination(int samplingIterations, float areaRadious, Transform centerPoint, out Vector3 destination);

    /// <summary>
    /// Sets the NavMeshAgent to be stopped or not.
    /// </summary>
    public void SetIfStopped(bool isStopped);

    /// <returns>If the NavMeshAgent is calculating a path to its destination.</returns>
    public bool IsPathPending();

    /// <returns>The target position in world coordinates.</returns>
    public Vector3 GetDestinationPos();
    #endregion

    #region ENERGY METHODS
    public void ReduceEnergy(float amount);
    public void IncreaseEnergy(float amount);
    public bool IsEnergyLow();
    public bool IsEnergyAtMax();
    #endregion
}
