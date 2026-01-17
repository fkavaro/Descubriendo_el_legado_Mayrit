using System;
using UnityEngine.AI;
using UnityEngine;

public interface INPC : ICharacter
{
    public enum RoleInConversation
    {
        Initiator,
        Follower,
        None
    }

    #region PROPERTIES HELPERS
    public NavMeshAgent Agent { get; }
    NPCMovementController MovementController { get; }
    NPCInteractionController InteractionController { get; }
    float AvoidanceRadius { get; }
    float MaxSamplingDistance { get; }
    int AvoidancePriorityVariance { get; }
    int BaseAvoidancePriority { get; }
    public float WalkSpeedVariance { get; }
    bool IsStopped { get; set; }
    RoleInConversation ConversationRole { get; set; }
    bool InAccessZone { get; set; }
    bool HasArrivedToMiddlePoint { get; set; }
    public INPC CurrentConversationTarget { get; set; }
    public GameObject CurrentConversationTargetGO { get; set; }
    public INPC LastConversationTarget { get; set; }
    public GameObject LastConversationTargetGO { get; set; }
    public float ConversationDuration { get; set; }
    public bool IsFollowingConversation { get; }
    #endregion

    #region METHODS
    /// <summary>
    /// Sets the NPC's full name.
    /// </summary>
    public void SetFullName(string given, string family);
    #endregion
}
