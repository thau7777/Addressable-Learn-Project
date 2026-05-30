using UnityEngine;

public class PlayerMove : State<PlayerController>
{
    public PlayerMove(PlayerController owner) : base(owner) { }

    public override void OnEnter() { }
    public override void OnExit() { }
    public override void Tick() { }

    public override void FixedTick()
    {
        Owner.Rb.MovePosition(Owner.Rb.position + Owner.InputDir * Owner.MoveSpeed * Time.fixedDeltaTime);
    }

    public override IState GetTransition()
    {
        if (Owner.InputDir.sqrMagnitude <= 0f) return Owner.SM.GetState<PlayerIdle>();
        return null;
    }
}
