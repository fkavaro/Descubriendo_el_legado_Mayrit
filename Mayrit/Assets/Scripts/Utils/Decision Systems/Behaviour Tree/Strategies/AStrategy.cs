using UnityEngine;

/// <summary>
/// AStrategy defines the contract for all strategies used in the behaviour tree nodes.
/// </summary>
public abstract class AStrategy
{
    protected readonly ANPC _npc;
    protected readonly LeafNode _leafNode;

    public AStrategy(ANPC npc, LeafNode leafNode)
    {
        _npc = npc;
        _leafNode = leafNode;
    }

    public abstract Node.Status Update();
    public virtual void Reset() { }
}
