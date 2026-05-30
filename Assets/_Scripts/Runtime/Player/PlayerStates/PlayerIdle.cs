using UnityEngine;

public class PlayerIdle : State<PlayerController>
{
    public PlayerIdle(PlayerController owner) : base(owner) { }

    public override void OnEnter() { }
    public override void OnExit() { }
    public override void Tick() { }

    public override IState GetTransition()
    {
        if (Owner.InputDir.sqrMagnitude > 0f) return Owner.SM.GetState<PlayerMove>();
        return null;
    }
}
