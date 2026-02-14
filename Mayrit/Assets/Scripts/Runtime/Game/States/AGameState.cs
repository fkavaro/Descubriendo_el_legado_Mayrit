using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public abstract class AGameState : AState
{
    protected ScenesController _scenesController;
    protected ProgressManager _progressManager;

    protected AGameState(string name)
    : base(name) { }

    public override void AwakeState()
    {
        _scenesController = ServiceLocator.Instance.Get<ScenesController>();
    }

    public override void StartState()
    {
        base.StartState();

        _progressManager = ServiceLocator.Instance.Get<ProgressManager>();
    }
}
