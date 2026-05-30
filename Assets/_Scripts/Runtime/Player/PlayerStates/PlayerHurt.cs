using UnityEngine;

public class PlayerHurt : State<PlayerController>
{
    private Vector3 _knockbackVelocity;
    private float _damping = 10f; // tốc độ decay

    public PlayerHurt(PlayerController owner) : base(owner) { }

    public override void OnEnter()
    {
        _knockbackVelocity = Owner.PendingKnockback;
    }
    public override void Tick()
    {
    }
    public override void FixedTick()
    {
        Owner.Rb.MovePosition(Owner.Rb.position + _knockbackVelocity * Time.fixedDeltaTime);
        _knockbackVelocity = Vector3.Lerp(_knockbackVelocity, Vector3.zero, _damping * Time.fixedDeltaTime);
    }
    public override IState GetTransition()
    {
        if (_knockbackVelocity.sqrMagnitude < 0.01f)
            return Owner.SM.GetState<PlayerIdle>();
        return null;
    }
    public override void OnExit()
    {
    }
}