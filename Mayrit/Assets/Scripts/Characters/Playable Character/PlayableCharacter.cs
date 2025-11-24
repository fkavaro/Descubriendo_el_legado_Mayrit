using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayableCharacter : ACharacter<FiniteStateMachine<APlayableCharacterState>>
{
    #region EDITOR PROPERTIES
    // Character information
    public AInformationSO _information;
    #endregion

    #region INTERNAL PROPERTIES
    public PlayableCharacterMovementController _playerController;
    FiniteStateMachine<APlayableCharacterState> _fsm;
    #endregion

    #region INHERITED
    public override FiniteStateMachine<APlayableCharacterState> InitializeBehaviourSystem()
    {
        _fsm = new(this);

        FreeRoam_PlayableCharacterState _freeRoamState = new(this);

        _fsm.SetInitialState(_freeRoamState);

        return _fsm;
    }
    #endregion

    #region MONOBEHAVIOUR
    protected override void Awake()
    {
        base.Awake();

        AnimationController = new(this, this, CharacterAnimator);
        _playerController = new(this, GetComponent<CharacterController>());
    }
    #endregion
}
