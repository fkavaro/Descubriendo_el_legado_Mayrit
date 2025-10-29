using UnityEngine;

public abstract class ABehaviourControllable : MonoBehaviour, IBehaviourControllable
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
