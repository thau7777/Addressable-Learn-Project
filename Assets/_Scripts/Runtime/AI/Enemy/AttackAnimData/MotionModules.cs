using System;
using PrimeTween;
using UnityEngine;

// Composable motion modules for StagedAttackAnimData. Each is a small, single-purpose
// [Serializable] class picked into a stage's [SerializeReference] module list.
// New motion type = new class here. Nothing else changes.

/// <summary>Rotate the visual root to (original euler + offset). Wind-up tilt / strike tilt / settle.</summary>
[Serializable]
public class RotateOffsetModule : IMotionModule
{
    public Vector3 eulerOffset;
    public Ease ease = Ease.OutQuad;

    public Sequence Build(in MotionContext ctx) =>
        Sequence.Create().Chain(Tween.LocalRotation(ctx.VisualRoot, Quaternion.Euler(ctx.OgEuler + eulerOffset), ctx.StageDuration, ease));
}

/// <summary>Spin the visual root N full turns about an axis (linear by default). Spin-lunge.</summary>
[Serializable]
public class SpinModule : IMotionModule
{
    public Vector3 axis = Vector3.up;
    [Min(0f)] public float turns = 1f;
    [Tooltip("Euler offset added to the original rotation to start the spin from (e.g. a wound-up angle).")]
    public Vector3 startEulerOffset;
    public Ease ease = Ease.Linear;

    public Sequence Build(in MotionContext ctx)
    {
        Vector3 start = ctx.OgEuler + startEulerOffset;
        Vector3 end = start + axis.normalized * 360f * turns;
        return Sequence.Create().Chain(Tween.LocalEulerAngles(ctx.VisualRoot, start, end, ctx.StageDuration, ease));
    }
}

/// <summary>Scale the visual root to (original scale × multiplier). Squash / stretch / settle.</summary>
[Serializable]
public class ScaleModule : IMotionModule
{
    public Vector3 multiplier = Vector3.one;
    public Ease ease = Ease.OutQuad;

    public Sequence Build(in MotionContext ctx) =>
        Sequence.Create().Chain(Tween.Scale(ctx.VisualRoot, Vector3.Scale(ctx.OgScale, multiplier), ctx.StageDuration, ease));
}

/// <summary>Two-phase Y squash-then-stretch within one stage. Use repeatCount on the stage to bounce ×N.</summary>
[Serializable]
public class BounceScaleModule : IMotionModule
{
    [Range(0f, 1f)] public float squatTimeRatio = 0.5f;
    [Tooltip("Y multiplier at the bottom of the squat (taller stage = lower value).")]
    public float squatYMultiplier = 0.7f;
    [Tooltip("Y multiplier at the top of the bounce.")]
    public float peakYMultiplier = 1.3f;
    public Ease squatEase = Ease.InQuad;
    public Ease peakEase = Ease.OutQuad;

    public Sequence Build(in MotionContext ctx)
    {
        float squatDur = ctx.StageDuration * squatTimeRatio;
        float peakDur = Mathf.Max(0f, ctx.StageDuration - squatDur);
        Vector3 squat = new(ctx.OgScale.x, ctx.OgScale.y * squatYMultiplier, ctx.OgScale.z);
        Vector3 peak = new(ctx.OgScale.x, ctx.OgScale.y * peakYMultiplier, ctx.OgScale.z);
        return Sequence.Create()
            .Chain(Tween.Scale(ctx.VisualRoot, squat, squatDur, squatEase))
            .Chain(Tween.Scale(ctx.VisualRoot, peak, peakDur, peakEase));
    }
}

/// <summary>Translate the owner along the locked direction, or back to the attack-start origin.</summary>
[Serializable]
public class TranslateModule : IMotionModule
{
    [Tooltip("Distance along the locked (flat) direction toward the target. Ignored if Return To Origin is set.")]
    public float distance = 3f;
    [Tooltip("Move the owner back to the world position it held at attack start (e.g. lunge recover).")]
    public bool returnToOrigin = false;
    public Ease ease = Ease.OutCubic;

    public Sequence Build(in MotionContext ctx)
    {
        Vector3 dest = returnToOrigin ? ctx.OriginWorldPos : ctx.OriginWorldPos + ctx.LockedDir * distance;
        return Sequence.Create().Chain(Tween.Position(ctx.OwnerTransform, dest, ctx.StageDuration, ease));
    }
}

/// <summary>Parabolic hop: up to an apex then down onto the target's ground position. Jump-land.</summary>
[Serializable]
public class ArcModule : IMotionModule
{
    public float height = 3f;
    [Range(0f, 1f)] public float apexTimeRatio = 0.6f;
    public Ease upEase = Ease.OutQuad;
    public Ease downEase = Ease.InQuad;

    public Sequence Build(in MotionContext ctx)
    {
        float upDur = ctx.StageDuration * apexTimeRatio;
        float downDur = Mathf.Max(0f, ctx.StageDuration - upDur);
        Vector3 apex = (ctx.OriginWorldPos + ctx.TargetGroundPos) * 0.5f + Vector3.up * height;
        return Sequence.Create()
            .Chain(Tween.Position(ctx.OwnerTransform, apex, upDur, upEase))
            .Chain(Tween.Position(ctx.OwnerTransform, ctx.TargetGroundPos, downDur, downEase));
    }
}
