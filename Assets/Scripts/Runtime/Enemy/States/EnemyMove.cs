using PrimeTween;
using UnityEngine;

public class EnemyMove : State<EnemyController>
{
    public EnemyMove(EnemyController owner) : base(owner)
    {
    }

    public override void OnEnter()
    {
        
    }
    public override void Tick()
    {
        Owner.RotateToPlayer();
    }
    public override void FixedTick()  // từ Tick() → FixedTick()
    {
        Owner.MovementStrategy.Move(Owner, Owner.PlayerTarget);
    }

    public override void OnExit() { }  // linearVelocity vô nghĩa với kinematic

    public override IState GetTransition()
    {
        if (Owner.PlayerTarget == null) return null;

        float dist = Vector3.Distance(Owner.transform.position, Owner.PlayerTarget.position);
        if (dist <= Owner.AttackStrategy.AttackData.attackRange)
            return Owner.SM.GetState<EnemyAttack>();

        return null;
    }


}
