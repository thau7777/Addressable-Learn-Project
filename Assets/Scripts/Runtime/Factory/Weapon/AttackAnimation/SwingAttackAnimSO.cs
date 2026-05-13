using PrimeTween;
using UnityEngine;

[CreateAssetMenu(fileName = "SwingAttackAnim", menuName = "Scriptable Objects/Weapon/Attack Animation/Swing")]
public class SwingAttackAnimSO : WeaponAttackAnimSO
{
    [SerializeField] private float _lungeDuration = 0.08f;
    [SerializeField, Min(0f)] private float _lungeOffset = 0.3f;
    [SerializeField] private float _swingDelay = 0f;
    [SerializeField] private float _swingAngle = 120f;
    [SerializeField] private float _swingDuration = 0.2f;
    [SerializeField] private float _returnDuration = 0.15f;
    [SerializeField] private Vector3 _swingAxis = new Vector3(1f, 1f, 0f);
    [SerializeField] private Ease _lungeEase = Ease.OutQuart;
    [SerializeField] private Ease _swingEase = Ease.OutQuad;
    [SerializeField] private Ease _returnEase = Ease.InOutQuad;

    public override Sequence Play(Transform weaponModel, Transform target, Vector3 originLocalPos, float range, Quaternion originLocalRot, float attackInterval)
    {
        Transform parent = weaponModel.parent;

        // Flat direction toward target (ignore Y)
        Vector3 worldDir = target != null ? (target.position - parent.position) : parent.forward;
        worldDir.y = 0f;

        // Actual flat distance to target; fall back to range when no target
        float distToTarget = worldDir.sqrMagnitude > 0.001f ? worldDir.magnitude : range;
        float lungeDistance = Mathf.Max(0f, distToTarget - _lungeOffset);

        if (worldDir.sqrMagnitude < 0.001f) worldDir = parent.forward;
        Vector3 localDir = parent.InverseTransformDirection(worldDir.normalized);

        Vector3 lungePos = originLocalPos + localDir * lungeDistance;
        // Swing around the model's own local X (transform.right) — adapts automatically as weapon faces target
        Quaternion swingRot = originLocalRot * Quaternion.AngleAxis(_swingAngle, _swingAxis.normalized);

        float scaledLungeDur   = _lungeDuration  * attackInterval;
        float scaledSwingDur   = _swingDuration  * attackInterval;
        float scaledSwingDelay = _swingDelay      * attackInterval;
        float scaledReturnDur  = _returnDuration  * attackInterval;

        Sequence swingSeq = Sequence.Create();
        if (scaledSwingDelay > 0f)
            swingSeq = swingSeq.Chain(Tween.Delay(scaledSwingDelay));
        swingSeq = swingSeq.Chain(Tween.LocalRotation(weaponModel, swingRot, scaledSwingDur, _swingEase));

        return Sequence.Create()
            .Group(Tween.LocalPosition(weaponModel, lungePos, scaledLungeDur, _lungeEase))
            .Group(swingSeq)
            .Chain(Tween.LocalPosition(weaponModel, originLocalPos, scaledReturnDur, _returnEase))
            .Group(Tween.LocalRotation(weaponModel, originLocalRot, scaledReturnDur, _returnEase));
    }
}
