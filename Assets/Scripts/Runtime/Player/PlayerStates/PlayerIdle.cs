using UnityEngine;

public class PlayerIdle : State<PlayerController>
{
    public PlayerIdle(PlayerController owner) : base(owner) { }

    public override void OnEnter() { /* idle anim */ }
    public override void OnExit() { }
    public override void Tick() { }

    public override void FixedTick()
    {
        Owner.Rb.linearVelocity = Vector3.zero;
    }

    public override IState GetTransition()
    {
        if (Owner.inputDir.sqrMagnitude > 0f) return Owner.SM.GetState<PlayerMove>();
        return null;
    }
}