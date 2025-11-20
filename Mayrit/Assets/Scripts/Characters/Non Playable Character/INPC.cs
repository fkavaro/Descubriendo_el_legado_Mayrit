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
    public string GivenName { get; }
    public string FamilyName { get; }
    #endregion

    #region METHODS
    /// <summary>
    /// Sets the NPC's full name.
    /// </summary>
    public void SetFullName(string given, string family);
    #endregion
}
