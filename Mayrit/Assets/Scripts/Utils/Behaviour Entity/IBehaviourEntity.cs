using UnityEngine;
using System.Collections;

public interface IBehaviourEntity
{
    #region PROPERTIES HELPERS
    string Name { get; }
    GameObject GO { get; }
    bool IsExecutionPaused { get; set; }
    string CurrentActionInfo { get; set; }
    #endregion

    public void StartCoroutine(IEnumerator enumerator);
}
