using UnityEngine;

public class PlayerHurt : State<PlayerController>
{
    [SerializeField] private float _knockbackDecay = 15f;
    private Vector3 _knockbackVelocity;
    private readonly float _exitThreshold = 0.05f;

    public PlayerHurt(PlayerController owner) : base(owner) { }

    public override void OnEnter()
    {
        _knockbackVelocity = Owner.PendingKnockback; // take ownership of the force
        // hurt anim, flash VFX
    }

    public override void OnExit() { }
    public override void Tick() { }

    public override void FixedTick()
    {
        Owner.Rb.linearVelocity = _knockbackVelocity;
        _knockbackVelocity = Vector3.MoveTowards(_knockbackVelocity, Vector3.zero, _knockbackDecay * Time.fixedDeltaTime);
    }

    public override IState GetTransition()
    {
        if (_knockbackVelocity.sqrMagnitude > _exitThreshold) return null;

        return Owner.inputDir.sqrMagnitude > 0f
            ? Owner.SM.GetState<PlayerMove>()
            : Owner.SM.GetState<PlayerIdle>();
    }
}