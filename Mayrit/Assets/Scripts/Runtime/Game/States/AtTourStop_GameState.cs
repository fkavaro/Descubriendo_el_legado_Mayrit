using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class AtTourStop_GameState : AGameState
{
    public TourStop TourStop;

    public AtTourStop_GameState(GameManager gameManager)
    : base(gameManager, "AtTourStop") { }

    public override void StartState()
    {
        base.StartState();

        UISystem.SwitchToInformationDisplayState(TourStop.Data);
        CameraSystem.SwitchToTourStopCamera(TourStop.Camera);
        PlayableCharacter.SwitchToAtTourStopState(TourStop);

        _gameManager.InputActions.UI.Enable();
    }

    public override void ExitState()
    {
        base.ExitState();

        _gameManager.InputActions.UI.Disable();
    }
}