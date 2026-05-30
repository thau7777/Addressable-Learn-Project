using LokiInspector;
using PrimeTween;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/AttackAnimData/BounceShootAnimData")]
public class BounceShootAnimData : AttackAnimData
{
    [TabGroup("Bounce")]
    [Min(1)] public int bounceCount = 3;
    [TabGroup("Bounce")]
    [LabelText("Wind-up Y multiplier (squat)")]
    public float windUpScaleY = 0.7f;
    [TabGroup("Bounce")]
    public float windUpDur = 0.15f;
    [TabGroup("Bounce")]
    [LabelText("Bounce-up Y multiplier (taller)")]
    public float bounceUpScaleY = 1.3f;
    [TabGroup("Bounce")]
    public float bounceUpDur = 0.15f;

    [TabGroup("Return")]
    public float returnDur = 0.2f;
    [TabGroup("Return")]
    public Ease returnEase = Ease.OutBack;

    public override IAttackAnimation Create() => new BounceShootAnim(this);
}

public class BounceShootAnim : IAttackAnimation
{
    private readonly BounceShootAnimData _cfg;
    private Sequence _sequence;

    public BounceShootAnim(BounceShootAnimData cfg) => _cfg = cfg;

    public void Build(IEnemyContext owner, Transform target, System.Action onStrike, System.Action onComplete, System.Action<bool> setFaceTarget)
    {
        var vr = owner.VisualRoot;
        var ogScale = owner.VrOgScale;
        Vector3 windUpScaleAbs = new Vector3(ogScale.x, ogScale.y * _cfg.windUpScaleY, ogScale.z);
        Vector3 bounceUpScaleAbs = new Vector3(ogScale.x, ogScale.y * _cfg.bounceUpScaleY, ogScale.z);

        _sequence = Sequence.Create();
        for (int i = 0; i < _cfg.bounceCount; i++)
        {
            _sequence = _sequence
                .Chain(Tween.Scale(vr, windUpScaleAbs, _cfg.windUpDur, Ease.InQuad))
                .Chain(Tween.Scale(vr, bounceUpScaleAbs, _cfg.bounceUpDur, Ease.OutQuad))
                .ChainCallback(onStrike);
        }

        _sequence = _sequence
            .Chain(Tween.Scale(vr, ogScale, _cfg.returnDur, _cfg.returnEase))
            .ChainCallback(onComplete);
    }

    public void OnInterrupt(IEnemyContext owner)
    {
        if (_sequence.isAlive) _sequence.Stop();
        owner.VisualRoot.localScale = owner.VrOgScale;
        owner.VisualRoot.localRotation = owner.VrOgRotation;
    }
}
