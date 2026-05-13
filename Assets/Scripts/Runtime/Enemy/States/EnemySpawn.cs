using PrimeTween;
using UnityEngine;

public class EnemySpawn : State<EnemyController>
{
    private Tween _tween;
    private bool _done;

    public EnemySpawn(EnemyController owner) : base(owner) { }

    public override void OnEnter()
    {
        _done = false;
        _tween = Tween.Scale(Owner.VisualRoot, Vector3.zero, Owner.VrOgScale, Owner.Data.spawnDuration, Ease.OutBack)
                      .OnComplete(() => _done = true);
    }

    public override void Tick() { }

    public override IState GetTransition()
    {
        return _done ? Owner.SM.GetState<EnemyIdle>() : null;
    }

    public override void OnExit()
    {
        _tween.Stop();
    }
}
