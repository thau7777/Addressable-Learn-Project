using PrimeTween;
using UnityEngine;

public class EnemyIdle : State<EnemyController>
{
    private float _timer;
    private Tween _tween;

    public EnemyIdle(EnemyController owner) : base(owner) { }

    public override void OnEnter()
    {
        _timer = 0f;

        // Squash & stretch in place — giống quả đang thở/nảy nhẹ
        _tween = Tween.Scale(Owner.VisualRoot,
            endValue: new Vector3(Owner.VrOgScale.x * 1.08f, Owner.VrOgScale.y * 0.88f, Owner.VrOgScale.z * 1.08f),
            duration: 0.55f,
            ease: Ease.InOutSine,
            cycles: -1,
            cycleMode: CycleMode.Yoyo);
    }

    public override void Tick()
    {
        _timer += Time.deltaTime;
        Owner.RotateToPlayer();
    }

    public override IState GetTransition()
    {
        if (_timer >= 2f && Owner.PlayerTarget != null)
            return Owner.SM.GetState<EnemyMove>();
        return null;
    }

    public override void OnExit()
    {
        _tween.Stop();
        Owner.VisualRoot.localScale = Owner.VrOgScale;
    }
}