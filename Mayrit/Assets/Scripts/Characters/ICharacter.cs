using UnityEngine;

public interface ICharacter : IBehaviourEntity
{
    public enum CharacterGender
    {
        Male,
        Female
    }

    #region PROPERTIES HELPERS
    public Animator CharacterAnimator { get; }
    public CharacterAnimationController AnimationController { get; set; }
    public GameObject CharacterModel { get; }
    public bool IsFemale { get; }
    float WalkSpeed { get; }
    float SprintSpeed { get; }
    float RotationSpeed { get; }
    float JumpForce { get; }
    float GravityForce { get; }
    Vector2 ArrivedDistance { get; }
    Vector2 NearDistance { get; }
    bool IsInteracting { get; set; }
    bool IsInStreet { get; set; }
    public ICharacter CurrentInteractionTarget { get; set; }
    public ICharacter LastInteractionTarget { get; set; }
    #endregion

    #region METHODS
    /// <summary>
    /// Returns true if the character is available to start an interaction.
    /// </summary>
    public bool IsAvailableForInteraction(ICharacter initiator);

    /// <summary>
    /// Called on the target character when an initiator requests interaction.
    /// Returns true if accepted and the target is now reserved for interaction.
    /// </summary>
    public bool TryAcceptInteraction(ICharacter initiator);

    /// <summary>
    /// Called on the initiator character to start the interaction
    /// </summary>
    public void StartInteraction();

    /// <summary>
    /// Ends an ongoing interaction on this character (called on both participants)
    /// </summary>
    public void EndInteraction();
    #endregion
}
