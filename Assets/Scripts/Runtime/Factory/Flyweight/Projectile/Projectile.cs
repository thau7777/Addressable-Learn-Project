using UnityEngine;

public abstract class Projectile : Flyweight
{
    [SerializeField]
    protected TrailRenderer _trail;
    private Collider _collider;
    protected GunData _gunData;

    protected bool _isInitialized;
    protected virtual void Awake()
    {
        _collider = GetComponent<Collider>();
        _trail = GetComponentInChildren<TrailRenderer>();
    }

    protected virtual void OnDisable()
    {
        _trail.Clear();
        _trail.enabled = false; // disable so it doesn't capture the teleport
    }

    protected void ResetTrail()
    {
        _trail.enabled = true;
    }
    public abstract void ShootProjectile(Vector3 startPos, Vector3 targetPos, GunData gunData);
    protected virtual void Despawn()
    {
        //_trail.End();
        _isInitialized = false;
        ReturnToPool();
    }
    protected virtual void OnTriggerEnter(Collider other)
    {
    }
}
