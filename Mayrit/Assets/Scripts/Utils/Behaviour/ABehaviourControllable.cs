using UnityEngine;

[RequireComponent(typeof(BehaviourController))]
public abstract class ABehaviourControllable : MonoBehaviour, IBehaviourControllable
{
    #region PROPERTIES
    public string Name => gameObject.name;
    public BehaviourController BehaviourController
    {
        get => _behaviourController;
        set => _behaviourController = value;
    }
    BehaviourController _behaviourController;
    #endregion

    #region MONOBEHAVIOUR
    protected virtual void Awake()
    {
        _behaviourController = GetComponent<BehaviourController>();
    }
    #endregion

}
