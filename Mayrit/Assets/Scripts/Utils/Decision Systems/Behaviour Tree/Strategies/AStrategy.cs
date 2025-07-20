
/// <summary>
/// AStrategy defines the contract for all strategies used in the behaviour tree nodes.
/// </summary>
public abstract class AStrategy<TController>
where TController : ABehaviourController<TController>
{
    protected readonly TController _controller;

    public AStrategy(TController controller)
    {
        _controller = controller;
    }

    public abstract Node<TController>.Status Update();
    public virtual void Reset() { }
}
