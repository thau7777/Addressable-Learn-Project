using PrimeTween;
using UnityEngine;

public class EnemySpawn : State<EnemyController>
{
    private Tween _tween;
    private float _timer;
    public EnemySpawn(EnemyController owner) : base(owner)
    {
    }


    public override void OnEnter()
    {
        _timer = 0f;
        _tween = Tween.Scale(Owner.VisualRoot, Vector3.zero, Owner.VrOgScale, Owner.Data.spawnDuration, Ease.OutBack);
    }


    public override void Tick()
    {
        _timer += Time.deltaTime;
    }
    public override IState GetTransition()
    {
        if (_timer >= Owner.Data.spawnDuration)
        {
            return Owner.SM.GetState<EnemyIdle>();
        }
        return null;
    }
    public override void OnExit()
    {
        _tween.Stop();
    }
}
