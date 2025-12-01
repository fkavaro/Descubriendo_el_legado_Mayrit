using System;
using System.Collections.Generic;
using UnityEngine;

public class AtPOI_PlayableCharacterState : APlayableCharacterState
{
    #region PROPERTY HELPERS
    public PointOfInterest CurrentPOI
    {
        get => _currentPOI;
        set => _currentPOI = value;
    }
    #endregion

    #region INTERNAL PROPERTIES
    PointOfInterest _currentPOI;
    bool _hasArrived,
        _isRotated;
    #endregion

    #region CONSTRUCTOR
    public AtPOI_PlayableCharacterState(PlayableCharacter playableCharacter)
    : base("At POI", playableCharacter) { }
    #endregion

    #region INHERITED
    public override void StartState()
    {
        // Reset flag
        _hasArrived = false;
    }

    public override void UpdateState()
    {
        // Return if arrived at POI and rotated as its camera
        if (_isRotated) return;

        // Look towards POI's camera direction when arrived to POI
        if (_hasArrived)
            _isRotated = _playableCharacter.MovementController.SmoothRotation(_currentPOI.Camera.transform.rotation);
        // Move towards POI
        else
            _hasArrived = _playableCharacter.MovementController.SetDestination(_currentPOI.transform.position);
    }
    #endregion
}
