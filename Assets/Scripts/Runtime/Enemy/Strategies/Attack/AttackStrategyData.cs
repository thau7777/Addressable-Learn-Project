using PrimeTween;
using UnityEngine;

// IAttackStrategy.cs
public interface IAttackStrategy
{
    AttackStrategyData AttackData { get; }
    bool IsReady { get; }
    void StartAttack(EnemyController owner, Transform target, System.Action onComplete); 
    void Interrupt(EnemyController owner);
}

public abstract class AttackStrategyData : ScriptableObject
{
    public float attackRange = 1.4f;
    public float damage = 10f;
    public float cooldown = 1f;
    public AttackAnimData animData; 

    public FlyweightSettings attackFlyweightSettings;
    public Vector3 positionOffset;
    public Quaternion rotationOffset;
    public abstract IAttackStrategy CreateStrategy();
}

