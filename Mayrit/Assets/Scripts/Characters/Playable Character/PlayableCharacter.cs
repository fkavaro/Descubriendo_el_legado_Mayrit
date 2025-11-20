using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayableCharacter : ACharacter<FiniteStateMachine>
{
    #region EDITOR PROPERTIES
    // Character information
    public AInformationSO _information;
    #endregion

    #region INTERNAL PROPERTIES
    public PlayableCharacterMovementController _playerController;
    FiniteStateMachine _fsm;
    #endregion

    #region INHERITED
    public override FiniteStateMachine InitializeBehaviourSystem()
    {
        _fsm = new(this);

        FreeRoam_PlayableCharacterState _freeRoamState = new(_fsm, this);

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
