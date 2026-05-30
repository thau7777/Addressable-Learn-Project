using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class Projectile : Flyweight
{
    [SerializeField]
    protected TrailRenderer _trail;
    protected Rigidbody _rb;
    protected Collider _collider;
    protected MeshRenderer[] _meshRenderers;
    protected IProjectileLaunchData _launchData;
    protected bool _isInitialized;

    private bool _enableInterpolationNextStep;
    private CancellationTokenSource _lifetimeCts;

    protected ProjectileSettings ProjectileSettings => settings as ProjectileSettings;

    protected virtual void Awake()
    {
        _trail = GetComponentInChildren<TrailRenderer>();
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        _meshRenderers = GetComponentsInChildren<MeshRenderer>();
    }

    protected virtual void FixedUpdate()
    {
        if (!_enableInterpolationNextStep || _rb == null) return;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _enableInterpolationNextStep = false;
    }

    protected void PrepareRigidbodyForLaunch(Vector3 pos, Quaternion rot)
    {
        if (_rb == null) return;
        _rb.interpolation = RigidbodyInterpolation.None;
        _rb.position = pos;
        _rb.rotation = rot;
        _enableInterpolationNextStep = true;
    }

    protected virtual void OnEnable()
    {
        _lifetimeCts = new CancellationTokenSource();
    }

    protected virtual void OnDisable()
    {
        if (_lifetimeCts != null)
        {
            _lifetimeCts.Cancel();
            _lifetimeCts.Dispose();
            _lifetimeCts = null;
        }
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
            damageable.TakeDamage(_launchData.Damage);
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
        _enableInterpolationNextStep = false;
        if (_rb != null) _rb.interpolation = RigidbodyInterpolation.None;
        if (_collider != null) _collider.enabled = false;
        foreach (var mr in _meshRenderers) mr.enabled = false;

        if (_trail != null && _trail.time > 0f)
            FadeTrailThenPool(_lifetimeCts.Token).Forget();
        else
            ReturnToPool();
    }

    private async UniTaskVoid FadeTrailThenPool(CancellationToken ct)
    {
        _trail.emitting = false;
        bool canceled = await UniTask.WaitForSeconds(_trail.time, cancellationToken: ct).SuppressCancellationThrow();
        if (canceled) return;
        ReturnToPool();
    }

    protected virtual void OnTriggerEnter(Collider other) { }
}
