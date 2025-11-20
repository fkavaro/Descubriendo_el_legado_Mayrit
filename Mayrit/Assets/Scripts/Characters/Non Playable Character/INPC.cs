using UnityEngine;
using UnityEngine.AI;

public interface INPC : ICharacter
{
    #region PROPERTIES HELPERS
    public NavMeshAgent Agent { get; }
    NPCMovementController MovementController { get; }
    float AvoidanceRadius { get; }
    float MaxSamplingDistance { get; }
    int AvoidancePriorityVariance { get; }
    int BaseAvoidancePriority { get; }
    bool IsStopped { get; set; }
    bool IsInStreet { get; set; }
    public string GivenName { get; }
    public string FamilyName { get; }
    #endregion

    #region PUBLIC METHODS
    /// <summary>
    /// Sets the NPC's full name.
    /// </summary>
    public void SetFullName(string given, string family);

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
    #endregion
}
