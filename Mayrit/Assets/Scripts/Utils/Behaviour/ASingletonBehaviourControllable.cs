using UnityEngine;

[RequireComponent(typeof(BehaviourController))]
public abstract class ASingletonBehaviourControllable<T> : Singleton<T>, IBehaviourControllable
where T : MonoBehaviour
{
    #region PROPERTIES
    public string Name => gameObject.name;
    public BehaviourController BehaviourController => _behaviourController;

    BehaviourController _behaviourController;
    #endregion

    #region MONOBEHAVIOUR
    protected override void Awake()
    {
        _behaviourController = GetComponent<BehaviourController>();
    }
    #endregion
}
