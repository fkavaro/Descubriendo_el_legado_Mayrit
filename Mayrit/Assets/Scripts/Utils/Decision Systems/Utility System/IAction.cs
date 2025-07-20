
public interface IAction
{
    public string Name { get; }
    public float Utility { get; }
    public abstract void StartAction();
    public abstract void UpdateAction();
    public virtual void FinishAction() { }
    public abstract bool IsFinished();
    public abstract string DebugDecision();
    public virtual void Reset() { }
}
