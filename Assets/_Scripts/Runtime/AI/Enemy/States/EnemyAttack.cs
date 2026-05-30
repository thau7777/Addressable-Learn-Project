using UnityEngine;

public class EnemyAttack : State<EnemyController>
{
    private bool _done;

    public EnemyAttack(EnemyController owner) : base(owner) { }

    public override void OnEnter()
    {
        if (Owner.PlayerTarget == null) return;
        _done = false;
        Owner.AttackStrategy.StartAttack(Owner, Owner.PlayerTarget, () => _done = true);
    }

    public override void Tick()
    {
        if (Owner.AttackStrategy.ShouldFaceTarget)
            Owner.RotateToPlayer();
    }

    public override IState GetTransition()
    {
        if (Owner.PlayerTarget != null && _done)
        {
            float dist = Vector3.Distance(Owner.transform.position, Owner.PlayerTarget.position);
            return dist <= Owner.AttackStrategy.AttackData.attackRange * 1.2f && Owner.AttackStrategy.IsReady
                ? Owner.SM.GetState<EnemyAttack>()
                : Owner.SM.GetState<EnemyIdle>();
        }

        return null;
    }

    public override void OnExit()
    {
        if (!_done)
            Owner.AttackStrategy.Interrupt(Owner);
    }
}
