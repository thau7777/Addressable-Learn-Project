using PrimeTween;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/AttackAnimData/StandardMeleeAnimData")]
public class StandardMeleeAnimData : AttackAnimData
{
    [Header("Wind-up")]
    public Vector3 windUpOffset = new Vector3(15f, 0f, 0f); // offset cộng thêm vào ogEuler
    public float windUpDur = 0.12f;

    [Header("Strike")]
    public Vector3 strikeOffset = new Vector3(-30f, 0f, 0f);
    public float strikeDur = 0.1f;
    public Vector3 squashScale = new Vector3(1.15f, 0.78f, 1.15f);
    public float impactHold = 0.08f;
    public float strikeSpawnDelay = 0.2f;

    [Header("Return")]
    public float returnDur = 0.22f;

    public override IAttackAnimation Create() => new StandardMeleeAnim(this);
}

public class StandardMeleeAnim : IAttackAnimation
{
    private readonly StandardMeleeAnimData _cfg;
    private Sequence _sequence;

    public StandardMeleeAnim(StandardMeleeAnimData cfg) => _cfg = cfg;

    public void Build(EnemyController owner, Transform target, System.Action onStrike, System.Action onComplete)
    {
        var vr = owner.VisualRoot;
        // ✅ Lấy eulerAngles từ localRotation đã lưu đúng
        var ogEuler = owner.VrOgRotation.eulerAngles;

        var windUpRot = Quaternion.Euler(ogEuler + _cfg.windUpOffset);
        var strikeRot = Quaternion.Euler(ogEuler + _cfg.strikeOffset);

        _sequence = Sequence.Create()
            .Chain(Tween.LocalRotation(vr, windUpRot, _cfg.windUpDur, Ease.OutQuad))

            .Chain(Tween.LocalRotation(vr, strikeRot, _cfg.strikeDur, Ease.InQuad))
            .Group(Tween.Scale(vr, _cfg.squashScale, _cfg.strikeDur, Ease.OutQuad))
            .InsertCallback(_cfg.strikeSpawnDelay, onStrike)
            .Chain(Tween.Delay(_cfg.impactHold))

            .Chain(Tween.LocalRotation(vr, owner.VrOgRotation, _cfg.returnDur, Ease.OutBack))
            .Group(Tween.Scale(vr, Vector3.one, _cfg.returnDur, Ease.OutBack))
            .ChainCallback(onComplete);
    }

    public void OnInterrupt(EnemyController owner)
    {
        if (_sequence.isAlive) _sequence.Stop();
        owner.VisualRoot.localRotation = owner.VrOgRotation;
        owner.VisualRoot.localScale = owner.VrOgScale;
    }
}