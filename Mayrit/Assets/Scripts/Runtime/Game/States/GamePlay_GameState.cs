using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class Gameplay_GameState : AGameState
{
    public FiniteStateMachine<AGameState> Fsm;

    public Gameplay_GameState(GameManager gameManager)
    : base(gameManager, "Gameplay") { }
}