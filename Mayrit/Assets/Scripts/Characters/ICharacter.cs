using UnityEngine;

public interface ICharacter : IBehaviourEntity
{
    public enum CharacterGender
    {
        Male,
        Female
    }

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
    public INPC CurrentInteractionTarget { get; set; }
}
