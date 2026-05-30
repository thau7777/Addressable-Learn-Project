using LokiInspector;
using PrimeTween;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/AttackAnimData/SpinLungeAnimData")]
public class SpinLungeAnimData : AttackAnimData
{
    [TabGroup("Wind-up")]
    public float windUpDur = 0.3f;
    [TabGroup("Wind-up")]
    public Vector3 windUpRotOffset = new Vector3(-15f, 0f, 0f);

    [TabGroup("Spin & Lunge")]
    [Min(1)] public int spinCount = 3;
    [TabGroup("Spin & Lunge")]
    public float spinDuration = 0.18f;
    [TabGroup("Spin & Lunge")]
    public float lungeDistance = 3f;
    [TabGroup("Spin & Lunge")]
    public Vector3 spinAxis = Vector3.up;
    [TabGroup("Spin & Lunge")]
    public Ease lungeEase = Ease.OutCubic;

    [TabGroup("Return")]
    public float returnDur = 0.35f;
    [TabGroup("Return")]
    public Ease returnEase = Ease.OutBack;

    public override IAttackAnimation Create() => new SpinLungeAnim(this);
}

public class SpinLungeAnim : IAttackAnimation
{
    private readonly SpinLungeAnimData _cfg;
    private Sequence _sequence;
    private Vector3 _originWorldPos;

    public SpinLungeAnim(SpinLungeAnimData cfg) => _cfg = cfg;

    public void Build(IEnemyContext owner, Transform target, System.Action onStrike, System.Action onComplete, System.Action<bool> setFaceTarget)
    {
        var vr = owner.VisualRoot;
        var ownerTr = owner.transform;
        var ogEuler = owner.VrOgRotation.eulerAngles;

        _originWorldPos = ownerTr.position;

        Vector3 lockedDir = target.position - _originWorldPos;
        lockedDir.y = 0f;
        if (lockedDir.sqrMagnitude < 0.0001f) lockedDir = ownerTr.forward;
        lockedDir.Normalize();
        Vector3 lungeEndPos = _originWorldPos + lockedDir * _cfg.lungeDistance;

        Vector3 windUpEuler = ogEuler + _cfg.windUpRotOffset;
        float totalSpinDur = _cfg.spinDuration * _cfg.spinCount;
        Vector3 spinEndEuler = ogEuler + _cfg.spinAxis.normalized * 360f * _cfg.spinCount;

        _sequence = Sequence.Create()
            .Chain(Tween.LocalRotation(vr, Quaternion.Euler(windUpEuler), _cfg.windUpDur, Ease.OutQuad))
            .ChainCallback(() => setFaceTarget(false))
            .Chain(Tween.Position(ownerTr, lungeEndPos, totalSpinDur, _cfg.lungeEase))
            .Group(Tween.LocalEulerAngles(vr, windUpEuler, spinEndEuler, totalSpinDur, Ease.Linear));

        // Insert offsets are measured from sequence start — strike at the top of each spin.
        float spinPhaseStart = _cfg.windUpDur;
        for (int i = 0; i < _cfg.spinCount; i++)
            _sequence = _sequence.InsertCallback(spinPhaseStart + i * _cfg.spinDuration, onStrike);

        _sequence = _sequence
            .Chain(Tween.Position(ownerTr, _originWorldPos, _cfg.returnDur, _cfg.returnEase))
            .Group(Tween.LocalRotation(vr, Quaternion.Euler(ogEuler), _cfg.returnDur, _cfg.returnEase))
            .ChainCallback(() => setFaceTarget(true))
            .ChainCallback(onComplete);
    }

    public void OnInterrupt(IEnemyContext owner)
    {
        if (_sequence.isAlive) _sequence.Stop();
        owner.transform.position = _originWorldPos;
        owner.VisualRoot.localRotation = owner.VrOgRotation;
        owner.VisualRoot.localScale = owner.VrOgScale;
    }
}
