using UnityEngine;
using UnityEngine.AI;

public interface INPC : IBehaviourEntity
{
    public enum NPCGender
    {
        Male,
        Female
    }

    #region PUBLIC METHODS
    public NavMeshAgent Agent { get; }
    public AnimationController AnimationController { get; }

    public string GivenName { get; }
    public string FamilyName { get; }
    public string FullName { get; }
    public NPCGender Gender { get; }
    public bool IsFemale { get; }

    /// <summary>
    /// Sets the NPC's full name.
    /// </summary>
    public void SetName(string given, string family);

    /// <summary>
    /// Sets the destination position for the NavMeshAgent to navigate to
    /// </summary>
    public void SetDestination(Vector3 destinationPos);
    /// <summary>
    /// Sets the destination spot for the NavMeshAgent to navigate to
    /// </summary>
    public void SetDestinationSpot(Spot destinationSpot);

    /// <returns>If the current destination spot is occupied</returns>
    public bool IsDestinationSpotOccupied();

    /// <summary>
    /// Checks if the NavMeshAgent is close to a certain position
    /// and makes it look at it when arriving, if wanted.
    /// </summary>
    public bool IsCloseTo(Vector3 destination, float horizontalDistance = 2f, float verticalDistance = 3.5f);
    /// <summary>
    /// Checks if the NavMeshAgent is close to a certain spot
    /// and makes it look at it when arriving, if wanted.
    /// </summary>
    public bool IsCloseTo(Spot spot, float horizontalDistance = 2f, float verticalDistance = 3.5f);
    /// <summary>
    /// Checks if the NavMeshAgent is close to its destination
    /// and makes it look at it when arriving, if wanted.
    /// </summary>
    public bool IsCloseToDestination(float horizontalDistance = 2f, float verticalDistance = 3.5f);

    /// <summary>
    /// Checks if the NavMeshAgent has arrived at its destination, 
    /// and if the target is a spot, fixes its rotation if wanted.
    /// </summary>
    public bool HasArrivedAtDestination(bool fixRotation = true, bool fixPosition = true);
    /// <summary>
    /// Checks if the NavMeshAgent has arrived at certain spot position
    /// and if the target is a spot, fixes its rotation if wanted.
    /// </summary>
    public bool HasArrivedAt(Spot spot, bool fixRotation = true, bool fixPosition = true);
    /// <summary>
    /// Checks if the NavMeshAgent has arrived at certain destination
    /// and if the target is a spot, fixes its rotation if wanted.
    /// </summary>
    public bool HasArrivedAt(Vector3 destination, bool fixRotation = true, bool fixPosition = true);

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

    public Vector3 GetDestinationPos();
    public Spot GetDestinationSpot();
    public bool IsDestination(Vector3 position);
    public bool IsDestination(Spot spot);
    void PlaceAt(Spot destinationSpot);
    void PlaceAt(Vector3 position);
    void PlaceAtDestination();

    /// <summary>
    /// Returns true if the NPC is available to start an interaction
    /// </summary>
    public bool IsAvailableForInteraction();

    /// <summary>
    /// Called on the target villager when an initiator requests interaction.
    /// Returns true if accepted and the target is now reserved for interaction.
    /// </summary>
    public bool TryAcceptInteraction(INPC initiator);

    /// <summary>
    /// Called on the initiator villager to start the interaction
    /// </summary>
    public void StartInteraction();

    /// <summary>
    /// Ends an ongoing interaction on this villager (called on both participants)
    /// </summary>
    public void EndInteraction();

    /// <summary>
    /// Expose current target for strategies to read
    /// </summary>
    public INPC CurrentInteractionTarget { get; }
    #endregion
}
