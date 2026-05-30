using System;
using LokiInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Strategy/Attack/BounceShootAttackData")]
public class BounceShootAttackData : AttackStrategyData
{
    public ProjectileSettings projectileSettings => attackFlyweightSettings as ProjectileSettings;
    [TabGroup("Projectile")]
    public float bulletSpeed = 15f;
    [TabGroup("Projectile")]
    public float bulletRange = 20f;
    [TabGroup("Projectile")]
    public Vector3 projectileSpawnLocalOffset = Vector3.zero;

    public override IAttackStrategy CreateStrategy() => new BounceShootAttack(this);
}

public class BounceShootAttack : IAttackStrategy
{
    public AttackStrategyData AttackData { get; private set; }
    public bool IsReady => _cooldownTimer <= 0f;
    public bool ShouldFaceTarget => _shouldFaceTarget;

    private readonly BounceShootAttackData _data;
    private readonly IAttackAnimation _anim;
    private float _cooldownTimer;
    private bool _shouldFaceTarget = true;
    private IEnemyContext _owner;
    private Transform _target;

    public BounceShootAttack(BounceShootAttackData data)
    {
        _data = data;
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
        _target = target;
        _cooldownTimer = AttackData.cooldown;
        _shouldFaceTarget = true;
        _anim.Build(owner, target, ShootProjectile, onComplete, SetFaceTarget);
    }

    public void Interrupt(IEnemyContext owner) => _anim.OnInterrupt(owner);

    private void SetFaceTarget(bool v) => _shouldFaceTarget = v;

    private void ShootProjectile()
    {
        if (_data.projectileSettings == null || _owner == null || _target == null) return;
        var fw = FlyweightFactory.Spawn(_data.projectileSettings);
        if (fw is not Projectile projectile) return;

        Vector3 spawnPos = _owner.transform.position + _owner.transform.TransformDirection(_data.projectileSpawnLocalOffset);
        projectile.FlyweightInit(spawnPos, _owner.transform.rotation);
        projectile.ShootProjectile(
            spawnPos,
            _target.position,
            new RuntimeProjectileLaunchData(_data.bulletSpeed, _owner.Damage, _data.bulletRange));
    }

    private readonly struct RuntimeProjectileLaunchData : IProjectileLaunchData
    {
        public RuntimeProjectileLaunchData(float speed, float damage, float range)
        {
            Speed = speed;
            Damage = damage;
            Range = range;
        }
        public float Speed { get; }
        public float Damage { get; }
        public float Range { get; }
    }
}
