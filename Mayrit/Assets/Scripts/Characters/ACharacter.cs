using UnityEngine;
using UnityEngine.TextCore.Text;

public abstract class ACharacter<T> : ABehaviourEntity<T>, ICharacter
where T : ABehaviourSystem
{
    #region PROPERTIES HELPERS
    public Animator CharacterAnimator => _animator;
    public CharacterAnimationController AnimationController
    {
        get => _animationController;
        set => _animationController = value;
    }
    public GameObject CharacterModel => _characterModel;
    public bool IsFemale => _gender == ICharacter.CharacterGender.Female;
    public float WalkSpeed => _walkSpeed;
    public float SprintSpeed => _sprintSpeed;
    public float RotationSpeed => _rotationSpeed;
    public float JumpForce => _jumpForce;
    public float GravityForce => _gravityForce;
    public Vector2 ArrivedDistance => _arrivedDistance;
    public Vector2 NearDistance => _nearDistance;
    public bool IsInteracting
    {
        get => _isInteracting;
        set => _isInteracting = value;
    }
    public INPC CurrentInteractionTarget
    {
        get => _currentInteractionTarget;
        set => _currentInteractionTarget = value;
    }
    #endregion

    #region EDITOR PROPERTIES
    [Header("Movement")]
    [SerializeField] protected float _walkSpeed = 1f;
    [SerializeField] protected float _sprintSpeed = 3f;
    [SerializeField] protected float _rotationSpeed = 4f;
    [SerializeField] protected float _jumpForce = 2f;
    [SerializeField] protected float _gravityForce = 9f;
    [Tooltip("Distance to which it's considered as arrived at destination (horizontal, vertical)")]
    [SerializeField] protected Vector2 _arrivedDistance = new(0.3f, 1.5f);
    [Tooltip("Distance to which it's considered as 'near' to a target (horizontal, vertical)")]
    [SerializeField] protected Vector2 _nearDistance = new(5f, 7f);

    [Header("Animation")]
    [SerializeField] protected Animator _animator;

    [Header("Interaction")]
    [Tooltip("Minimum range to start an interaction with another character")]
    [SerializeField] protected float _interactionRange = 3f;
    [Tooltip("Cooldown time between interactions with other characters")]
    [SerializeField] protected float _interactionCooldown = 0f;

    [Header("Character information")]
    [SerializeField] protected ICharacter.CharacterGender _gender = ICharacter.CharacterGender.Male;
    [SerializeField] protected GameObject _characterModel;
    #endregion

    #region INTERNAL PROPERTIES   
    CharacterAnimationController _animationController;
    bool _isInteracting = false;
    INPC _currentInteractionTarget;
    #endregion
}
