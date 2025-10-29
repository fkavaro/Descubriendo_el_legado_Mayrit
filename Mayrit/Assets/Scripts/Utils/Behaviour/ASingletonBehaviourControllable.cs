using UnityEngine;

public abstract class ASingletonBehaviourControllable<T> : Singleton<T>, IBehaviourControllable
where T : MonoBehaviour
{
    #region PROPERTIES
    public string Name => gameObject.name;

    public ADecisionSystem DecisionSystem => _decisionSystem;
    protected ADecisionSystem _decisionSystem;
    #endregion

    #region METHODS
    public abstract void SetDecisionSystem();
    #endregion
}
