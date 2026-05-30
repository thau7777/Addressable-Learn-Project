using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class StraightProjectile : Projectile
{
    private StraightProjectileSettings StraightProjectileSettings => settings as StraightProjectileSettings;
    private float _currentDistance;
    private Vector3 _prevPosition;

    protected override void Awake()
    {
        base.Awake();
        _rb.linearDamping = 0;
        _rb.angularDamping = 0;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _rb.useGravity = false;
        _rb.constraints = RigidbodyConstraints.FreezeRotation;
        _currentDistance = 0;
    }

    protected override void Despawn()
    {
        _rb.linearVelocity = Vector3.zero;
        base.Despawn();
    }

    public override void ShootProjectile(Vector3 startPos, Vector3 targetPos, IProjectileLaunchData launchData)
    {
        if (_collider != null) _collider.enabled = true;
        _launchData = launchData;
        Vector3 direction = (targetPos - startPos).normalized;

        PrepareRigidbodyForLaunch(startPos, Quaternion.LookRotation(direction));
        _rb.linearVelocity = direction * _launchData.Speed;
        _prevPosition = startPos;
        _currentDistance = 0;
        _isInitialized = true;

        ResetTrail();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!_isInitialized) return;

        Vector3 currentPos = _rb.position;
        Vector3 delta = currentPos - _prevPosition;
        float stepDistance = delta.magnitude;

        if (stepDistance > 0f && Physics.Raycast(_prevPosition, delta.normalized, out RaycastHit hit, stepDistance, StraightProjectileSettings.collisionLayers))
        {
            OnHit(hit.collider);
            SpawnImpactVfx(hit.point);
            Despawn();
            return;
        }

        _prevPosition = currentPos;
        _currentDistance += stepDistance;

        if (_currentDistance >= _launchData.Range)
        {
            Despawn();
            SpawnImpactVfx();
        }
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (!_isInitialized) return;
        if ((StraightProjectileSettings.collisionLayers.value & (1 << other.gameObject.layer)) == 0) return;
        OnHit(other);
        SpawnImpactVfx();
        Despawn();
    }
}
