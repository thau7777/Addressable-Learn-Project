using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class StraightProjectile : Projectile
{
    private StraightProjectileSettings StraightProjectileSettings => settings as StraightProjectileSettings;
    private Rigidbody _rb;
    private float _currentDistance;

    protected override void Awake()
    {
        base.Awake();
        _rb = GetComponent<Rigidbody>();
        _rb.linearDamping = 0;
        _rb.angularDamping = 0;
        _rb.interpolation = RigidbodyInterpolation.None;
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
        Quaternion rot = Quaternion.LookRotation(direction);

        _rb.position = startPos;
        _rb.rotation = rot;
        _rb.linearVelocity = direction * _launchData.BulletSpeed;
        _currentDistance = 0;
        _isInitialized = true;

        ResetTrail();
    }

    private void FixedUpdate()
    {
        if (!_isInitialized) return;
        _currentDistance += _rb.linearVelocity.magnitude * Time.fixedDeltaTime;
        if (_currentDistance >= _launchData.WeaponRange)
        {
            Despawn();
            SpawnImpactVfx();
        }
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if ((StraightProjectileSettings.collisionLayers.value & (1 << other.gameObject.layer)) == 0) return;
        OnHit(other);
        SpawnImpactVfx();
        Despawn();
    }
}
