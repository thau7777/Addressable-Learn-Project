using System;
using Cysharp.Threading.Tasks;
using LokiInspector;
using UnityEngine;

/// <summary>
/// The single data-driven enemy attack strategy. Cooldown + face-target plumbing is shared;
/// the only per-archetype variation — what each strike spawns — is composed via an
/// <see cref="IStrikeSpawner"/>. Replaces StandardMelee/SpinLunge/JumpLand/BounceShoot strategies.
/// </summary>
[CreateAssetMenu(menuName = "Scriptable Objects/Strategy/Attack/StagedAttackData")]
public class StagedAttackData : AttackStrategyData
{
    [TabGroup("Strike")]
    [SerializeReferenceDropdown]
    [SerializeReference] public IStrikeSpawner strikeSpawner;

    public override IAttackStrategy CreateStrategy() => new StagedAttack(this);

    public override async UniTask PreloadAsync()
    {
        if (strikeSpawner != null) await strikeSpawner.PreloadAsync();
    }
}

public class StagedAttack : IAttackStrategy
{
    public AttackStrategyData AttackData { get; private set; }
    public bool IsReady => _cooldownTimer <= 0f;
    public bool ShouldFaceTarget => _shouldFaceTarget;

    private readonly StagedAttackData _data;
    private readonly IAttackAnimation _anim;
    private float _cooldownTimer;
    private bool _shouldFaceTarget = true;
    private IEnemyContext _owner;
    private Transform _target;

    public StagedAttack(StagedAttackData data)
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
        _anim.Build(owner, target, OnStrike, onComplete, SetFaceTarget);
    }

    public void Interrupt(IEnemyContext owner) => _anim.OnInterrupt(owner);

    private void SetFaceTarget(bool v) => _shouldFaceTarget = v;

    private void OnStrike() => _data.strikeSpawner?.Spawn(_owner, _target);
}
