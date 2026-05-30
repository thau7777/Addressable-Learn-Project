using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Strategy/Movement/Ranged")]
public class RangedMovementData : MovementStrategyData
{
    public float preferredDistance = 6f;
    public float tolerance = 0.8f;
    public override IMovementStrategy CreateStrategy() => new RangedMovement(preferredDistance, tolerance);
}

public class RangedMovement : IMovementStrategy
{
    private readonly float _preferred;
    private readonly float _tolerance;

    public RangedMovement(float preferred, float tolerance)
    {
        _preferred = preferred;
        _tolerance = tolerance;
    }

    public void Move(IEnemyContext owner, Transform target)
    {
        if (target == null) return;
        float dist = Vector3.Distance(owner.transform.position, target.position);
        Vector3 dir = (target.position - owner.transform.position).normalized;
        dir.y = 0f;

        Vector3 moveDir = Vector3.zero;
        if (dist > _preferred + _tolerance) moveDir = dir;
        else if (dist < _preferred - _tolerance) moveDir = -dir;

        owner.Rb.MovePosition(owner.Rb.position + moveDir * owner.Data.moveSpeed * Time.fixedDeltaTime);
    }
}
