using UnityEngine;

[RequireComponent(typeof(BehaviourController))]
public abstract class ABehaviourControllable : MonoBehaviour, IBehaviourControllable
{
    #region PROPERTIES
    public string Name => gameObject.name;
    public BehaviourController BehaviourController => _behaviourController;

    BehaviourController _behaviourController;
    #endregion

    #region MONOBEHAVIOUR
    protected virtual void Awake()
    {
        _behaviourController = GetComponent<BehaviourController>();
    }
    #endregion

}
