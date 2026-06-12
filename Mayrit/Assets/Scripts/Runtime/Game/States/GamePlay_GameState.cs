using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class Gameplay_GameState : AGameState
{
    public FiniteStateMachine<AGameState> Fsm;

    Aerial_GameState _aerialState;
    ThirdPerson_GameState _thirdPersonState;
    AtPOI_GameState _atPOIState;
    AtTourStop_GameState _atTourStopState;
    AtCollectible_GameState _atCollectibleState;

    public Gameplay_GameState(GameManager gameManager)
    : base(gameManager, "Gameplay") { }

    public override void AwakeState()
    {
        base.AwakeState();

        _aerialState = new(_gameManager);
        _thirdPersonState = new(_gameManager);
        _atPOIState = new(_gameManager);
        _atTourStopState = new(_gameManager);
        _atCollectibleState = new(_gameManager);

        Fsm = new(_gameManager);
    }

    public override void StartState()
    {
        base.StartState();

        Fsm.CurrentState?.StartState();
    }

    public void SwitchToAerialState() => Fsm.SwitchState(_aerialState);
    public bool IsInAerialState() => Fsm.IsCurrentState(_aerialState);
    public Aerial_GameState AerialState => _aerialState;

    public void SwitchToThirdPersonState() => Fsm.SwitchState(_thirdPersonState);
    public bool IsInThirdPersonState() => Fsm.IsCurrentState(_thirdPersonState);
    public ThirdPerson_GameState ThirdPersonState => _thirdPersonState;

    public void SwitchToAtPOIState(DataSO data, OrbitalCameraSettings orbitalCameraSettings)
    {
        _atPOIState.Data = data;
        _atPOIState.OrbitalCameraSettings = orbitalCameraSettings;
        Fsm.SwitchState(_atPOIState);
    }
    public bool IsInAtPOIState() => Fsm.IsCurrentState(_atPOIState);
    public AtPOI_GameState AtPOIState => _atPOIState;

    public void SwitchToAtTourStopState(TourStop tourStop)
    {
        _atTourStopState.TourStop = tourStop;
        Fsm.SwitchState(_atTourStopState);
    }
    public bool IsInAtTourStopState() => Fsm.IsCurrentState(_atTourStopState);
    public AtTourStop_GameState AtTourStopState => _atTourStopState;

    public void SwitchToAtCollectibleState(Collectible collectible)
    {
        _atCollectibleState.Collectible = collectible;
        Fsm.SwitchState(_atCollectibleState);
    }
    public bool IsInAtCollectibleState() => Fsm.IsCurrentState(_atCollectibleState);
    public AtCollectible_GameState AtCollectibleState => _atCollectibleState;
}