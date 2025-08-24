using UnityEngine;

public abstract class ADecisionSystem<TController>
where TController : MonoBehaviour
{
    public readonly ABehaviourController<TController> _controller;


    public ADecisionSystem(ABehaviourController<TController> controller)
    {
        _controller = controller;
        _controller._decisionSystem = this;
    }

    protected abstract void DebugDecision();

    public virtual void Awake() { }
    public virtual void Start() { }
    public abstract void Update();
    public virtual void LateUpdate() { }

    public abstract void Reset();

    public virtual void OnCollisionEnter(Collision collision) { }
    public virtual void OnCollisionStay(Collision collision) { }
    public virtual void OnCollisionExit(Collision collision) { }

    public virtual void OnTriggerEnter(Collider other) { }
    public virtual void OnTriggerStay(Collider other) { }
    public virtual void OnTriggerExit(Collider other) { }


}