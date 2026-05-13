using System.Collections;
using UnityEngine;

public abstract class Projectile : Flyweight
{
    [SerializeField]
    protected TrailRenderer _trail;
    protected Collider _collider;
    protected MeshRenderer[] _meshRenderers;
    protected IProjectileLaunchData _launchData;
    protected bool _isInitialized;

    protected ProjectileSettings ProjectileSettings => settings as ProjectileSettings;

    protected virtual void Awake()
    {
        _trail = GetComponentInChildren<TrailRenderer>();
        _collider = GetComponent<Collider>();
        _meshRenderers = GetComponentsInChildren<MeshRenderer>();
    }

    protected virtual void OnDisable()
    {
        if (_trail == null) return;
        _trail.Clear();
        _trail.emitting = true;
        _trail.enabled = false;
    }

    protected void ResetTrail()
    {
        foreach (var mr in _meshRenderers) mr.enabled = true;
        if (_trail == null) return;
        _trail.Clear();
        _trail.emitting = true;
        _trail.enabled = true;
    }

    public abstract void ShootProjectile(Vector3 startPos, Vector3 targetPos, IProjectileLaunchData launchData);

    protected virtual void OnHit(Collider other)
    {
        if (ProjectileSettings.dealsDamage && other.TryGetComponent<IDamageable>(out var damageable))
            damageable.TakeDamage(_launchData.BulletDamage);
    }

    protected virtual void SpawnImpactVfx() => SpawnImpactVfx(transform.position);

    protected virtual void SpawnImpactVfx(Vector3 position)
    {
        if (ProjectileSettings.onImpactVfx == null) return;
        var vfx = FlyweightFactory.Spawn(ProjectileSettings.onImpactVfx) as OneShotVfx;
        if (vfx == null) return;
        vfx.FlyweightInit(position, transform.rotation);
        vfx.OneShotVfxInit();
    }

    protected virtual void Despawn()
    {
        _isInitialized = false;
        if (_collider != null) _collider.enabled = false;
        foreach (var mr in _meshRenderers) mr.enabled = false;

        if (_trail != null && _trail.time > 0f)
            StartCoroutine(FadeTrailThenPool());
        else
            ReturnToPool();
    }

    private IEnumerator FadeTrailThenPool()
    {
        _trail.emitting = false;
        yield return new WaitForSeconds(_trail.time);
        ReturnToPool();
    }

    protected virtual void OnTriggerEnter(Collider other) { }
}
