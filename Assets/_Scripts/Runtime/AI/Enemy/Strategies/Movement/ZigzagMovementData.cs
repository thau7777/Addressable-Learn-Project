using LokiInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Strategy/Movement/Zigzag")]
public class ZigzagMovementData : MovementStrategyData
{
    [TabGroup("Dash")]
    [LabelText("Speed Multiplier", LabelTextAttribute.LabelColor.cyan)]
    public float dashMultiplier = 3f;

    [TabGroup("Dash")]
    [LabelText("Dash Distance")]
    public float dashDistance = 3f;

    [TabGroup("Dash")]
    [LabelText("Lateral Offset")]
    public float lateralOffset = 1.5f;

    [TabGroup("Pause")]
    [LabelText("Base Duration", LabelTextAttribute.LabelColor.cyan)]
    public float pauseDuration = 0.5f;

    [TabGroup("Pause")]
    [LabelText("Speed Reduction Factor")]
    public float pauseReductionFactor = 0.1f;

    [TabGroup("Pause")]
    [LabelText("Min Duration")]
    public float minPauseDuration = 0.05f;

    [TabGroup("Trail")]
    [Required]
    [LabelText("Trail Prefab", LabelTextAttribute.LabelColor.cyan)]
    public GameObject trailPrefab;

    [TabGroup("Trail")]
    public Vector3 positionOffset;

    public override IMovementStrategy CreateStrategy() => new ZigzagMovement(this);
}

public class ZigzagMovement : IMovementStrategy, IMovementLifecycle, IMovementStateListener
{
    private enum ZigzagPhase { ZigDash, ZigPause, ZagDash, ZagPause }

    private readonly ZigzagMovementData _data;

    private ZigzagPhase _phase;
    private float _distanceTraveled;
    private float _pauseTimer;
    private TrailResetter _trailResetter;

    public ZigzagMovement(ZigzagMovementData data)
    {
        _data = data;
    }

    public void OnOwnerCreated(IEnemyContext owner)
    {
        var trailGo = Object.Instantiate(_data.trailPrefab, owner.transform, false);
        trailGo.GetComponent<TrailRenderer>().enabled = false;
        trailGo.transform.localPosition = _data.positionOffset;
        _trailResetter = trailGo.GetOrAdd<TrailResetter>();
    }

    public void OnOwnerReset()
    {
        _phase = ZigzagPhase.ZigDash;
        _distanceTraveled = 0f;
        _pauseTimer = 0f;
    }

    public void OnMoveEnter(IEnemyContext owner) => _trailResetter?.Activate();
    public void OnMoveExit(IEnemyContext owner) => _trailResetter?.Deactivate();

    public void Move(IEnemyContext owner, Transform target)
    {
        if (target == null) return;

        float dt = Time.fixedDeltaTime;
        float moveSpeed = owner.Data.moveSpeed;

        switch (_phase)
        {
            case ZigzagPhase.ZigDash:
                PerformDash(owner, target, 1f, moveSpeed, dt);
                break;
            case ZigzagPhase.ZigPause:
                TickPause(dt, ZigzagPhase.ZagDash);
                break;
            case ZigzagPhase.ZagDash:
                PerformDash(owner, target, -1f, moveSpeed, dt);
                break;
            case ZigzagPhase.ZagPause:
                TickPause(dt, ZigzagPhase.ZigDash);
                break;
        }
    }

    private void PerformDash(IEnemyContext owner, Transform target, float lateralSign, float moveSpeed, float dt)
    {
        Vector3 forward = target.position - owner.transform.position;
        forward.y = 0f;
        forward.Normalize();
        Vector3 right = new Vector3(-forward.z, 0f, forward.x);
        Vector3 dir = (forward + right * (lateralSign * _data.lateralOffset)).normalized;

        float step = moveSpeed * _data.dashMultiplier * dt;
        owner.Rb.MovePosition(owner.Rb.position + dir * step);
        _distanceTraveled += step;

        if (_distanceTraveled >= _data.dashDistance)
        {
            _distanceTraveled = 0f;
            _pauseTimer = CalcPauseDuration(moveSpeed);
            _phase = _phase == ZigzagPhase.ZigDash ? ZigzagPhase.ZigPause : ZigzagPhase.ZagPause;
        }
    }

    private void TickPause(float dt, ZigzagPhase nextPhase)
    {
        _pauseTimer -= dt;
        if (_pauseTimer <= 0f)
            _phase = nextPhase;
    }

    private float CalcPauseDuration(float moveSpeed) =>
        Mathf.Max(_data.minPauseDuration, _data.pauseDuration / (1f + moveSpeed * _data.pauseReductionFactor));
}
