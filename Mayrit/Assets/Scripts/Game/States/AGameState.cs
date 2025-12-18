using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public abstract class AGameState : AState
{
    protected AGameState(string name)
    : base(name) { }
}
