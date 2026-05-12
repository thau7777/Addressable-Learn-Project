using PrimeTween;
using UnityEngine;

// IAttackAnimation.cs
public interface IAttackAnimation
{
    void Build(EnemyController owner, Transform target, System.Action onStrike, System.Action onComplete);
    void OnInterrupt(EnemyController owner);
}

public abstract class AttackAnimData : ScriptableObject
{
    public abstract IAttackAnimation Create();
}

