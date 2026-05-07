
public abstract class State<TOwner> : IState
{
    protected TOwner Owner { get; }
    protected State(TOwner owner) => Owner = owner;
    public abstract void OnEnter();
    public abstract void Tick();
    public virtual void FixedTick() { }       // optional override
    public abstract IState GetTransition();
    public abstract void OnExit();

}
