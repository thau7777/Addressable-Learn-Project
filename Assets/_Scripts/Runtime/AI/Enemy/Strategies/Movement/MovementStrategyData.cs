using UnityEngine;

public interface IMovementStrategy
{
    void Move(IEnemyContext owner, Transform target);
}

public abstract class MovementStrategyData : ScriptableObject
{
    public abstract IMovementStrategy CreateStrategy();
}
