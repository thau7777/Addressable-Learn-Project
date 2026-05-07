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
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; // Better collision detection for fast-moving objects
        _rb.useGravity = false; // No gravity for straight projectiles
        _rb.constraints = RigidbodyConstraints.FreezeRotation; // Prevent rotation for a straight projectile
        _currentDistance = 0;
    }
    public override void ShootProjectile(Vector3 startPos, Vector3 targetPos, GunData gunData)
    {
        //_trail.Begin();
        _gunData = gunData;
        Vector3 direction = (targetPos - startPos).normalized;
        Quaternion rot = Quaternion.LookRotation(direction);

        _rb.position = startPos;
        _rb.rotation = rot;
        _rb.linearVelocity = direction * _gunData.BulletSpeed;
        _currentDistance = 0;
        _isInitialized = true;

        ResetTrail();
    }
    private void FixedUpdate()
    {
        if (!_isInitialized) return;
        _currentDistance += _rb.linearVelocity.magnitude * Time.fixedDeltaTime;
        if (_currentDistance >= _gunData.BulletRange)
        {
            Despawn();
        }
    }


    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        Despawn();
    }
}
