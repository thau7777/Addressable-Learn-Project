public class PlayerMove : State<PlayerController>
{
    public PlayerMove(PlayerController owner) : base(owner) { }

    public override void OnEnter() { /* move anim */ }
    public override void OnExit() { }
    public override void Tick() { }

    public override void FixedTick()
    {
        Owner.Rb.linearVelocity = Owner.inputDir * Owner.MoveSpeed;
    }

    public override IState GetTransition()
    {
        if (Owner.inputDir.sqrMagnitude <= 0f) return Owner.SM.GetState<PlayerIdle>();
        return null;
    }
}