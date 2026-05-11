using PrimeTween;
using UnityEngine;

public abstract class WeaponAttackAnimSO : ScriptableObject
{
    public abstract Sequence Play(Transform weaponModel, Transform target, Vector3 originLocalPos, Quaternion originLocalRot, float attackInterval);
}
