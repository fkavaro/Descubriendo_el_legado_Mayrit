using UnityEngine;

public interface IBehaviourEntity
{
    #region PROPERTIES HELPERS
    string Name { get; }
    GameObject GO { get; }
    bool IsExecutionPaused { get; set; }
    string CurrentActionInfo { get; set; }
    #endregion
}
