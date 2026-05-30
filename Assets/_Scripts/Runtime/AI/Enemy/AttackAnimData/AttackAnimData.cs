using System;
using UnityEngine;

public interface IAttackAnimation
{
    void Build(IEnemyContext owner, Transform target, Action onStrike, Action onComplete, Action<bool> setFaceTarget);
    void OnInterrupt(IEnemyContext owner);
}

public abstract class AttackAnimData : ScriptableObject
{
    public abstract IAttackAnimation Create();
}
