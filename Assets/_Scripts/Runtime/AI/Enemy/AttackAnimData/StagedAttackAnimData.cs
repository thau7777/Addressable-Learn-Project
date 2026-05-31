using System;
using System.Collections.Generic;
using LokiInspector;
using PrimeTween;
using UnityEngine;

/// <summary>
/// Data-driven attack animation: three stages (wind-up / attack / return), each a list of
/// composable motion modules run in parallel over the stage duration. Replaces the bespoke
/// per-enemy anim SOs. New motion = new <see cref="IMotionModule"/>, zero edits here.
/// </summary>
[CreateAssetMenu(menuName = "Scriptable Objects/AttackAnimData/StagedAttackAnimData")]
public class StagedAttackAnimData : AttackAnimData
{
    [TabGroup("Wind-up")] public AttackStage windUp = new() { name = "Wind-up", duration = 0.15f };
    [TabGroup("Attack")] public AttackStage attack = new() { name = "Attack", duration = 0.15f, strikesPerRepeat = 1, strikeTime = 1f };
    [TabGroup("Return")] public AttackStage returnStage = new() { name = "Return", duration = 0.2f };

    public override IAttackAnimation Create() => new StagedAttackAnim(this);
}

public enum FacingControl
{
    /// <summary>Leave the strategy's face-target flag untouched.</summary>
    Inherit,
    /// <summary>setFaceTarget(false) at stage start — lock the current facing (e.g. spin/jump start).</summary>
    Lock,
    /// <summary>setFaceTarget(true) at stage start — resume facing the target (e.g. return start).</summary>
    Unlock
}

[Serializable]
public class AttackStage
{
    [Tooltip("Label only — not used at runtime.")]
    public string name;
    [Min(0f)] public float duration = 0.15f;
    [Tooltip("Repeat the whole stage N times (e.g. bounce ×N).")]
    [Min(1)] public int repeatCount = 1;
    [Tooltip("onStrike callbacks per repeat (1 melee/jump, N spin/bounce).")]
    [Min(0)] public int strikesPerRepeat = 0;
    [Tooltip("Where in each strike slot the callback fires: 0 = slot start, 1 = slot end.")]
    [Range(0f, 1f)] public float strikeTime = 1f;
    public FacingControl facing = FacingControl.Inherit;
    [SerializeReferenceDropdown]
    [SerializeReference] public List<IMotionModule> modules = new();
}

/// <summary>Snapshot of everything a motion module needs, rebuilt per stage with that stage's duration.</summary>
public readonly struct MotionContext
{
    public readonly Transform VisualRoot;
    public readonly Transform OwnerTransform;
    public readonly Vector3 OgEuler;          // VisualRoot original local euler
    public readonly Vector3 OgScale;          // VisualRoot original local scale
    public readonly Vector3 OriginWorldPos;   // owner world pos at attack start
    public readonly Vector3 LockedDir;        // flat normalized dir to target at start
    public readonly Vector3 TargetGroundPos;  // (target.x, owner.y, target.z) at start
    public readonly float StageDuration;

    public MotionContext(Transform visualRoot, Transform ownerTransform, Vector3 ogEuler, Vector3 ogScale,
        Vector3 originWorldPos, Vector3 lockedDir, Vector3 targetGroundPos, float stageDuration)
    {
        VisualRoot = visualRoot;
        OwnerTransform = ownerTransform;
        OgEuler = ogEuler;
        OgScale = ogScale;
        OriginWorldPos = originWorldPos;
        LockedDir = lockedDir;
        TargetGroundPos = targetGroundPos;
        StageDuration = stageDuration;
    }
}

/// <summary>A single composable motion. Returns a PrimeTween sequence spanning the stage duration.</summary>
public interface IMotionModule
{
    Sequence Build(in MotionContext ctx);
}

public class StagedAttackAnim : IAttackAnimation
{
    private readonly StagedAttackAnimData _cfg;
    private Sequence _seq;

    // Geometry snapshot for the in-flight attack (one attack per enemy at a time).
    private Transform _vr;
    private Transform _ownerTr;
    private Vector3 _ogEuler;
    private Vector3 _ogScale;
    private Vector3 _originWorldPos;
    private Vector3 _lockedDir;
    private Vector3 _targetGround;

    public StagedAttackAnim(StagedAttackAnimData cfg) => _cfg = cfg;

    public void Build(IEnemyContext owner, Transform target, Action onStrike, Action onComplete, Action<bool> setFaceTarget)
    {
        _vr = owner.VisualRoot;
        _ownerTr = owner.transform;
        _ogEuler = owner.VrOgRotation.eulerAngles;
        _ogScale = owner.VrOgScale;
        _originWorldPos = _ownerTr.position;

        Vector3 dir = target.position - _originWorldPos;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) dir = _ownerTr.forward;
        _lockedDir = dir.normalized;
        _targetGround = new Vector3(target.position.x, _originWorldPos.y, target.position.z);

        _seq = Sequence.Create();
        AppendStage(_cfg.windUp, onStrike, setFaceTarget);
        AppendStage(_cfg.attack, onStrike, setFaceTarget);
        AppendStage(_cfg.returnStage, onStrike, setFaceTarget);
        _seq = _seq.ChainCallback(onComplete);
    }

    private void AppendStage(AttackStage stage, Action onStrike, Action<bool> setFaceTarget)
    {
        if (stage == null) return;
        bool hasModules = stage.modules != null && stage.modules.Count > 0;
        if (!hasModules && stage.strikesPerRepeat <= 0 && stage.duration <= 0f) return;

        if (stage.facing == FacingControl.Lock) _seq = _seq.ChainCallback(() => setFaceTarget(false));
        else if (stage.facing == FacingControl.Unlock) _seq = _seq.ChainCallback(() => setFaceTarget(true));

        var ctx = new MotionContext(_vr, _ownerTr, _ogEuler, _ogScale, _originWorldPos, _lockedDir, _targetGround, stage.duration);
        int repeats = Mathf.Max(1, stage.repeatCount);

        for (int r = 0; r < repeats; r++)
        {
            Sequence block = Sequence.Create();
            bool first = true;

            if (hasModules)
            {
                foreach (var m in stage.modules)
                {
                    if (m == null) continue;
                    Sequence contribution = m.Build(ctx);
                    block = first ? block.Chain(contribution) : block.Group(contribution);
                    first = false;
                }
            }

            // No motion — still consume the stage duration so strikes land at the right time.
            if (first) block = block.Chain(Tween.Delay(Mathf.Max(0.0001f, stage.duration)));

            int strikes = stage.strikesPerRepeat;
            if (strikes > 0)
            {
                float slot = stage.duration / strikes;
                for (int k = 0; k < strikes; k++)
                {
                    float offset = Mathf.Clamp((k + stage.strikeTime) * slot, 0f, stage.duration);
                    block = block.InsertCallback(offset, onStrike);
                }
            }

            _seq = _seq.Chain(block);
        }
    }

    public void OnInterrupt(IEnemyContext owner)
    {
        if (_seq.isAlive) _seq.Stop();
        owner.transform.position = _originWorldPos;
        owner.VisualRoot.localRotation = owner.VrOgRotation;
        owner.VisualRoot.localScale = owner.VrOgScale;
    }
}
