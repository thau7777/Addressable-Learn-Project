// IMovementStrategy.cs
using UnityEngine;

public interface IMovementStrategy
{
    void Move(EnemyController owner, Transform target);
}

// MovementStrategyData.cs
public abstract class MovementStrategyData : ScriptableObject
{
    public abstract IMovementStrategy CreateStrategy();
}