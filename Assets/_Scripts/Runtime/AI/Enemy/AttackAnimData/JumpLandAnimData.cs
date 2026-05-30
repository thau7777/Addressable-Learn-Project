using LokiInspector;
using PrimeTween;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/AttackAnimData/JumpLandAnimData")]
public class JumpLandAnimData : AttackAnimData
{
    [TabGroup("Wind-up")]
    public float windUpDur = 0.35f;
    [TabGroup("Wind-up")]
    [LabelText("Wind-up Scale (multiplier)")]
    public Vector3 windUpScale = new Vector3(1.2f, 0.6f, 1.2f);

    [TabGroup("Jump")]
    public float jumpUpDur = 0.3f;
    [TabGroup("Jump")]
    public float jumpDownDur = 0.2f;
    [TabGroup("Jump")]
    public float jumpHeight = 3f;
    [TabGroup("Jump")]
    public float hangTimeAtApex = 0f;
    [TabGroup("Jump")]
    public Ease jumpEaseUp = Ease.OutQuad;
    [TabGroup("Jump")]
    public Ease jumpEaseDown = Ease.InQuad;

    [TabGroup("Land")]
    [LabelText("Land Scale (multiplier)")]
    public Vector3 landScale = new Vector3(1.3f, 0.7f, 1.3f);
    [TabGroup("Land")]
    public float landPauseDur = 0.2f;

    [TabGroup("Return")]
    public float returnDur = 0.25f;
    [TabGroup("Return")]
    public Ease returnEase = Ease.OutBack;

    public override IAttackAnimation Create() => new JumpLandAnim(this);
}

public class JumpLandAnim : IAttackAnimation
{
    private readonly JumpLandAnimData _cfg;
    private Sequence _sequence;
    private Vector3 _originWorldPos;
    private Vector3 _landPos;

    public JumpLandAnim(JumpLandAnimData cfg) => _cfg = cfg;

    public void Build(IEnemyContext owner, Transform target, System.Action onStrike, System.Action onComplete, System.Action<bool> setFaceTarget)
    {
        var vr = owner.VisualRoot;
        var ownerTr = owner.transform;
        var ogScale = owner.VrOgScale;

        _originWorldPos = ownerTr.position;
        _landPos = new Vector3(target.position.x, _originWorldPos.y, target.position.z);

        Vector3 midpoint = (_originWorldPos + _landPos) * 0.5f + Vector3.up * _cfg.jumpHeight;
        Vector3 windUpScaleAbs = Vector3.Scale(ogScale, _cfg.windUpScale);
        Vector3 landScaleAbs = Vector3.Scale(ogScale, _cfg.landScale);

        _sequence = Sequence.Create()
            // Wind-up squat
            .Chain(Tween.Scale(vr, windUpScaleAbs, _cfg.windUpDur, Ease.OutQuad))
            // Lock landing direction once the jump starts
            .ChainCallback(() => setFaceTarget(false))
            // Jump up to apex; scale releases back to og
            .Chain(Tween.Position(ownerTr, midpoint, _cfg.jumpUpDur, _cfg.jumpEaseUp))
            .Group(Tween.Scale(vr, ogScale, _cfg.jumpUpDur, Ease.OutQuad));

        if (_cfg.hangTimeAtApex > 0f)
            _sequence = _sequence.Chain(Tween.Delay(_cfg.hangTimeAtApex));

        _sequence = _sequence
            // Fall to landing
            .Chain(Tween.Position(ownerTr, _landPos, _cfg.jumpDownDur, _cfg.jumpEaseDown))
            // Land: strike + landing squash
            .ChainCallback(onStrike)
            .Chain(Tween.Scale(vr, landScaleAbs, 0.05f, Ease.OutQuad))
            // Returning anim pause: hold the squash
            .Chain(Tween.Delay(_cfg.landPauseDur))
            // Wobble back to original scale via OutBack (the recover)
            .Chain(Tween.Scale(vr, ogScale, _cfg.returnDur, _cfg.returnEase))
            .ChainCallback(() => setFaceTarget(true))
            .ChainCallback(onComplete);
    }

    public void OnInterrupt(IEnemyContext owner)
    {
        if (_sequence.isAlive) _sequence.Stop();
        owner.transform.position = _originWorldPos;
        owner.VisualRoot.localScale = owner.VrOgScale;
        owner.VisualRoot.localRotation = owner.VrOgRotation;
    }
}
