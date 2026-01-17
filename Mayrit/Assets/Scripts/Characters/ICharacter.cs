using System;
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
    public CharacterAnimationController AnimationController { get; }
    public GameObject CharacterModel { get; }
    public bool IsFemale { get; }
    public string GivenName { get; }
    public string FamilyName { get; }
    float WalkSpeed { get; }
    float SprintSpeed { get; }
    float RotationSpeed { get; }
    float JumpForce { get; }
    float GravityForce { get; }
    float StoppingDistance { get; }
    float NearDistance { get; }
    #endregion
}
