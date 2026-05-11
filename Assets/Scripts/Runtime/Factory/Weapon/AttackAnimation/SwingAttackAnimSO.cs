using PrimeTween;
using UnityEngine;

[CreateAssetMenu(fileName = "SwingAttackAnim", menuName = "Scriptable Objects/Weapon/Attack Animation/Swing")]
public class SwingAttackAnimSO : WeaponAttackAnimSO
{
    [SerializeField] private float _lungeDistance = 0.4f;
    [SerializeField] private float _lungeDuration = 0.08f;
    [SerializeField] private Vector3 _swingAxis = Vector3.up;
    [SerializeField] private float _swingAngle = 120f;
    [SerializeField] private float _swingDuration = 0.2f;
    [SerializeField] private float _returnDuration = 0.15f;
    [SerializeField] private Ease _lungeEase = Ease.OutQuart;
    [SerializeField] private Ease _returnEase = Ease.InOutQuad;

    public override Sequence Play(Transform weaponModel, Transform target, Vector3 originLocalPos, Quaternion originLocalRot, float attackInterval)
    {
        Vector3 lungePos = originLocalPos + Vector3.forward * _lungeDistance;
        Quaternion swingRot = originLocalRot * Quaternion.AngleAxis(_swingAngle, _swingAxis);

        // lunge + swing start together, then both return simultaneously
        return Sequence.Create()
            .Group(Tween.LocalPosition(weaponModel, lungePos, _lungeDuration, _lungeEase))
            .Group(Tween.LocalRotation(weaponModel, swingRot, _swingDuration, _lungeEase))
            .Chain(Tween.LocalPosition(weaponModel, originLocalPos, _returnDuration, _returnEase))
            .Group(Tween.LocalRotation(weaponModel, originLocalRot, _returnDuration, _returnEase));
    }
}
