using UnityEngine;

/// <summary>
/// AStrategy defines the contract for all strategies used in the behaviour tree nodes.
/// </summary>
public abstract class AStrategy<TController>
where TController : MonoBehaviour
{
    protected readonly ANPC<TController> _controller;

    public AStrategy(ANPC<TController> controller)
    {
        _controller = controller;
    }

    public abstract Node<TController>.Status Update();
    public virtual void Reset() { }
}
