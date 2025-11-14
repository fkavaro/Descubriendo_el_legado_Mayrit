using UnityEngine;

public interface IBehaviourEntity
{
    string Name { get; }
    GameObject GO { get; }
    bool IsExecutionPaused { get; set; }
    string CurrentActionInfo { get; set; }
}
