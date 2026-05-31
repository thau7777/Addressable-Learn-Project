using Cysharp.Threading.Tasks;
using LokiInspector;
using System;
using UnityEngine;

public interface IAttackStrategy
{
    AttackStrategyData AttackData { get; }
    bool IsReady { get; }
    bool ShouldFaceTarget { get; }
    void Tick(float dt);
    void StartAttack(IEnemyContext owner, Transform target, Action onComplete);
    void Interrupt(IEnemyContext owner);
}

public abstract class AttackStrategyData : ScriptableObject
{
    public float attackRange = 1.4f;
    public float cooldown = 1f;

    [Required] public AttackAnimData animData;

    public abstract IAttackStrategy CreateStrategy();

    /// <summary>Preload any pooled assets this strategy spawns (hitboxes / projectiles). Called by EnemyData.LoadPrefabAsync.</summary>
    public virtual UniTask PreloadAsync() => UniTask.CompletedTask;
}
