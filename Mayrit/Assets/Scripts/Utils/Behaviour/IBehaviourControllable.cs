
/// <summary>
/// Interface for objects controlled by a behaviour controller.
/// </summary>
public interface IBehaviourControllable
{
    string Name { get; }
    BehaviourController BehaviourController { get; }
}
