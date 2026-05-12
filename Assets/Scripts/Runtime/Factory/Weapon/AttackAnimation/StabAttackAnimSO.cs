using PrimeTween;
using UnityEngine;

[CreateAssetMenu(fileName = "StabAttackAnim", menuName = "Scriptable Objects/Weapon/Attack Animation/Stab")]
public class StabAttackAnimSO : WeaponAttackAnimSO
{
    [SerializeField] private float _stabDuration = 0.08f;
    [SerializeField] private float _returnDuration = 0.15f;
    [SerializeField] private Ease _stabEase = Ease.OutQuad;
    [SerializeField] private Ease _returnEase = Ease.InQuad;

    public override Sequence Play(Transform weaponModel, Transform target, Vector3 originLocalPos, float range, Quaternion originLocalRot, float attackInterval)
    {
        Vector3 stabPos = originLocalPos + Vector3.forward * range;
        return Sequence.Create()
            .Chain(Tween.LocalPosition(weaponModel, stabPos, _stabDuration, _stabEase))
            .Chain(Tween.LocalPosition(weaponModel, originLocalPos, _returnDuration, _returnEase));
    }
}
