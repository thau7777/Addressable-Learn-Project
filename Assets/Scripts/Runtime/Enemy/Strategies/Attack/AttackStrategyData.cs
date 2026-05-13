using LokiInspector;
using System;
using UnityEngine;

public interface IAttackStrategy
{
    AttackStrategyData AttackData { get; }
    bool IsReady { get; }
    void Tick(float dt);
    void StartAttack(IEnemyContext owner, Transform target, Action onComplete);
    void Interrupt(IEnemyContext owner);
}

public abstract class AttackStrategyData : ScriptableObject
{
    public float attackRange = 1.4f;
    public float damage = 10f;
    public float cooldown = 1f;

    [Required] public AttackAnimData animData;

    public FlyweightSettings attackFlyweightSettings;
    public Vector3 positionOffset;
    public Quaternion rotationOffset;

    public abstract IAttackStrategy CreateStrategy();
}
