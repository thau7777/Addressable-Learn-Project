using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Strategy/Attack/StandardMeleeAttackData")]
public class StandardMeleeAttackData : AttackStrategyData
{
    public override IAttackStrategy CreateStrategy() => new StandardMeleeAttack(this);
}

public class StandardMeleeAttack : IAttackStrategy
{
    public AttackStrategyData AttackData { get; private set; }
    private readonly IAttackAnimation _anim;
    public bool IsReady => _cooldownTimer <= 0f;
    private float _cooldownTimer;

    public StandardMeleeAttack(StandardMeleeAttackData attackData)
    {
        AttackData = attackData;
        _anim = attackData.animData.Create();
    }

    public void Tick(float dt)
    {
        if (_cooldownTimer > 0f) _cooldownTimer -= dt;
    }

    public void StartAttack(IEnemyContext owner, Transform target, Action onComplete)
    {
        _cooldownTimer = AttackData.cooldown;
        _anim.Build(owner, target, SpawnAttackFlyweight, onComplete);
    }

    public void Interrupt(IEnemyContext owner) => _anim.OnInterrupt(owner);

    private void SpawnAttackFlyweight()
    {
        // TODO: spawn attack hitbox flyweight dealing AttackData.damage
    }
}
