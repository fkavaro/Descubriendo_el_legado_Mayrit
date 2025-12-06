using System;
using UnityEngine.AI;

public interface INPC : ICharacter
{
    public enum RoleInConversation
    {
        Initiator,
        Follower,
        None
    }

    #region PROPERTIES HELPERS
    public event Action ConversationFinishedEvent;

    public NavMeshAgent Agent { get; }
    NPCMovementController MovementController { get; }
    float AvoidanceRadius { get; }
    float MaxSamplingDistance { get; }
    int AvoidancePriorityVariance { get; }
    int BaseAvoidancePriority { get; }
    public float WalkSpeedVariance { get; }
    bool IsStopped { get; set; }
    public string GivenName { get; }
    public string FamilyName { get; }
    bool IsInStreet { get; set; }
    RoleInConversation ConversationRole { get; set; }
    bool ShouldTalk { get; set; }
    bool IsReadyToTalk { get; set; }
    public INPC CurrentConversationTarget { get; set; }
    public INPC LastConversationTarget { get; set; }
    #endregion

    #region METHODS
    /// <summary>
    /// Sets the NPC's full name.
    /// </summary>
    public void SetFullName(string given, string family);

    /// <summary>
    /// Returns true if is in the street and its model is active.
    /// </summary>
    /// <returns></returns>
    public bool IsAvailableForConversation();

    public bool IsStillInConversation(INPC otherNpc);

    /// <summary>
    /// Returns true if the character is available to start an interaction.
    /// </summary>
    public bool CanAcceptConversation(INPC initiator);

    /// <summary>
    /// Called on the initiator character to start the interaction
    /// </summary>
    public void Talk();

    /// <summary>
    /// Ends an ongoing interaction on this character (called on both participants)
    /// </summary>
    public void EndConversation();
    #endregion
}
