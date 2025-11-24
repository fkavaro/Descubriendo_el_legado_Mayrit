using UnityEngine;

public interface IBehaviourSystem
{
    #region PROPERTIES HELPERS
    public IBehaviourEntity BehaviourEntity { get; }
    #endregion

    public abstract void DebugDecision();
    public abstract void Reset();
    public virtual void Awake() { }
    public virtual void Start() { }
    public virtual void Update() { }
    public virtual void LateUpdate() { }
    public virtual void OnCollisionEnter(Collision collision) { }
    public virtual void OnCollisionStay(Collision collision) { }
    public virtual void OnCollisionExit(Collision collision) { }
    public virtual void OnTriggerEnter(Collider other) { }
    public virtual void OnTriggerStay(Collider other) { }
    public virtual void OnTriggerExit(Collider other) { }
}
