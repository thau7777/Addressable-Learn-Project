using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Strategy/Movement/Chaser")]
public class ChaserMovementData : MovementStrategyData
{
    public override IMovementStrategy CreateStrategy() => new ChaserMovement();
}

public class ChaserMovement : IMovementStrategy
{
    public void Move(IEnemyContext owner, Transform target)
    {
        if (target == null) return;
        Vector3 dir = (target.position - owner.transform.position).normalized;
        dir.y = 0f;
        owner.Rb.MovePosition(owner.Rb.position + dir * owner.Data.moveSpeed * Time.fixedDeltaTime);
    }
}
