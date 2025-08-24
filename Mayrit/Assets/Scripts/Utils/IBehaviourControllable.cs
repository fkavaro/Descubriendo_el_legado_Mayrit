
/// <summary>
/// Interface for objects controlled by a behaviour controller.
/// </summary>
public interface IBehaviourControllable
{
    string Name { get; }
    bool DebugMode { get; set; }
    bool IsExecutionPaused { get; set; }
}
