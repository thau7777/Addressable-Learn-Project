using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Strategy/Attack/SpinLungeAttackData")]
public class SpinLungeAttackData : AttackStrategyData
{
    public override IAttackStrategy CreateStrategy() => new SpinLungeAttack(this);
}

public class SpinLungeAttack : IAttackStrategy
{
    public AttackStrategyData AttackData { get; private set; }
    public bool IsReady => _cooldownTimer <= 0f;
    public bool ShouldFaceTarget => _shouldFaceTarget;

    private readonly IAttackAnimation _anim;
    private float _cooldownTimer;
    private bool _shouldFaceTarget = true;
    private IEnemyContext _owner;

    public SpinLungeAttack(SpinLungeAttackData data)
    {
        AttackData = data;
        _anim = data.animData.Create();
    }

    public void Tick(float dt)
    {
        if (_cooldownTimer > 0f) _cooldownTimer -= dt;
    }

    public void StartAttack(IEnemyContext owner, Transform target, Action onComplete)
    {
        _owner = owner;
        _cooldownTimer = AttackData.cooldown;
        _shouldFaceTarget = true;
        _anim.Build(owner, target, SpawnStrikeVfx, onComplete, SetFaceTarget);
    }

    public void Interrupt(IEnemyContext owner) => _anim.OnInterrupt(owner);

    private void SetFaceTarget(bool v) => _shouldFaceTarget = v;

    private void SpawnStrikeVfx()
    {
        if (AttackData.attackFlyweightSettings == null || _owner == null) return;
        var fw = FlyweightFactory.Spawn(AttackData.attackFlyweightSettings);
        if (fw == null) return;
        Vector3 pos = _owner.transform.position + _owner.transform.TransformDirection(AttackData.positionOffset);
        fw.FlyweightInit(pos, _owner.transform.rotation);
        if (fw is OneShotVfx vfx) vfx.OneShotVfxInit(_owner.Damage);
    }
}
