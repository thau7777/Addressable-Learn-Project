using PrimeTween;
using UnityEngine;

public abstract class Pickable : Flyweight
{
    private const float BaseAttractSpeed = 2f;
    private const float AttractAcceleration = 18f;
    private const float MaxAttractSpeed = 22f;
    private const float PickupDistance = 0.25f;

    protected PickableSettings PickableSettings => settings as PickableSettings;

    private Transform _attractor;
    private bool _isAttracted;
    private float _currentSpeed;

    private void OnEnable()
    {
        Tween.LocalRotation(transform, Quaternion.Euler(0, 360, 0), 1, cycleMode: CycleMode.Incremental);
        _attractor = null;
        _isAttracted = false;
        _currentSpeed = BaseAttractSpeed;
    }

    private void Update()
    {
        if (!_isAttracted || _attractor == null) return;

        _currentSpeed = Mathf.Min(_currentSpeed + AttractAcceleration * Time.deltaTime, MaxAttractSpeed);
        transform.position = Vector3.MoveTowards(transform.position, _attractor.position, _currentSpeed * Time.deltaTime);

        if ((_attractor.position - transform.position).sqrMagnitude <= PickupDistance * PickupDistance)
        {
            OnPicked();
        }
    }

    public void Attract(Transform attractor)
    {
        if (_attractor != null) return;
        _attractor = attractor;
        Tween.Position(transform, transform.position + Vector3.up * 0.5f, 0.5f)
            .OnComplete(OnAttractStart);
    }

    private void OnAttractStart()
    {
        _isAttracted = true;
    }

    protected virtual void OnPicked()
    {
        ReturnToPool();
    }
}
