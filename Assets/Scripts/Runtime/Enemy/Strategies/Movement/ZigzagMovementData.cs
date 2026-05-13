using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Strategy/Movement/Zigzag")]
public class ZigzagMovementData : MovementStrategyData
{
    public float frequency = 2.5f;
    public float amplitude = 2f;
    public override IMovementStrategy CreateStrategy() => new ZigzagMovement(frequency, amplitude);
}

public class ZigzagMovement : IMovementStrategy
{
    private float _timer;
    private readonly float _frequency;
    private readonly float _amplitude;

    public ZigzagMovement(float frequency, float amplitude)
    {
        _frequency = frequency;
        _amplitude = amplitude;
    }

    public void Move(IEnemyContext owner, Transform target)
    {
        if (target == null) return;
        _timer += Time.fixedDeltaTime;
        Vector3 forward = (target.position - owner.transform.position).normalized;
        forward.y = 0f;
        Vector3 right = new Vector3(-forward.z, 0f, forward.x);
        Vector3 dir = (forward + right * (Mathf.Sin(_timer * _frequency) * _amplitude)).normalized;
        owner.Rb.MovePosition(owner.Rb.position + dir * owner.Data.moveSpeed * Time.fixedDeltaTime);
    }
}
