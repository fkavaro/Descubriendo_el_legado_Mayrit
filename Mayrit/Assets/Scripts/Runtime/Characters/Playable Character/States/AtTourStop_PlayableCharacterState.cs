using System;
using System.Collections.Generic;
using UnityEngine;

public class AtTourStop_PlayableCharacterState : APlayableCharacterState
{
    #region PROPERTY HELPERS
    public TourStop CurrentTourStop
    {
        get => _currentTourStop;
        set => _currentTourStop = value;
    }
    #endregion

    #region INTERNAL PROPERTIES
    TourStop _currentTourStop;
    bool _hasArrived,
        _isRotated;
    #endregion

    #region CONSTRUCTOR
    public AtTourStop_PlayableCharacterState(PlayableCharacter playableCharacter)
    : base("At tour stop", playableCharacter) { }
    #endregion

    #region INHERITED
    public override void StartState()
    {
        // Reset flag
        _hasArrived = false;
        _isRotated = false;
    }

    public override void UpdateState()
    {
        // Return if arrived at TourStop and rotated as its camera
        if (_isRotated) return;

        // Look in tour stops's camera direction when arrived to it
        if (_hasArrived)
            _isRotated = _playableCharacter.MovementController.SmoothRotation(_currentTourStop.Camera.transform.rotation);
        // Move towards tour stop
        else
            _hasArrived = _playableCharacter.MovementController.SetDestination(_currentTourStop.transform.position);
    }
    #endregion
}
