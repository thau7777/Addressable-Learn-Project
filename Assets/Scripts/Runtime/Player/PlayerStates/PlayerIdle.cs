using UnityEngine;

public class PlayerIdle : State<PlayerController>
{
    public PlayerIdle(PlayerController owner) : base(owner) { }

    public override void OnEnter()
    {
        //Owner.Rb.MovePosition(Vector3.zero);
    }
    public override void OnExit() { }
    public override void Tick() { }


    public override IState GetTransition()
    {
        if (Owner.inputDir.sqrMagnitude > 0f) return Owner.SM.GetState<PlayerMove>();
        return null;
    }
}