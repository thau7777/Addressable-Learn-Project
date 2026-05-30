using System.Threading;
using Cysharp.Threading.Tasks;
using LokiInspector;
using UnityEngine;
#if UNITY_VFX_GRAPH
using UnityEngine.VFX;
#endif

public class OneShotVfx : Flyweight
{
    private OneShotVfxSettings Settings => settings as OneShotVfxSettings;

    private ParticleSystem _ps;
#if UNITY_VFX_GRAPH
    private VisualEffect _vfx;
#endif
    private Collider _hitbox;
    private float _currentDamage;
    private CancellationTokenSource _lifetimeCts;

    private void Awake()
    {
        _ps = GetComponentInChildren<ParticleSystem>();
#if UNITY_VFX_GRAPH
        _vfx = GetComponentInChildren<VisualEffect>();
#endif
        _hitbox = GetComponent<Collider>();
        if (_hitbox != null) _hitbox.enabled = false;
    }


    public void OneShotVfxInit(float damageOverride)
    {
        OneShotVfxInit();
        _currentDamage = damageOverride;
    }

    public void OneShotVfxInit()
    {
#if UNITY_VFX_GRAPH
        if (_ps == null && _vfx == null)
#else
        if (_ps == null)
#endif
        {
            Debug.LogWarning($"[OneShotVfx] {name} has no ParticleSystem or VisualEffect — skipping.", this);
            return;
        }

        gameObject.SetActive(true);
        _currentDamage = Settings.baseDamage;

        if (_ps != null) _ps.Play();
#if UNITY_VFX_GRAPH
        if (_vfx != null) _vfx.Play();
#endif

        _lifetimeCts = new CancellationTokenSource();
        var ct = _lifetimeCts.Token;

        if (Settings.dealsDamage && _hitbox != null)
            HitboxAsync(ct).Forget();

        WaitThenDespawnAsync(ct).Forget();
    }

    private void OnDisable()
    {
        if (_lifetimeCts != null)
        {
            _lifetimeCts.Cancel();
            _lifetimeCts.Dispose();
            _lifetimeCts = null;
        }
        if (_hitbox != null) _hitbox.enabled = false;

        if (_ps != null) _ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
#if UNITY_VFX_GRAPH
        if (_vfx != null) _vfx.Stop();
#endif
    }

    private async UniTaskVoid HitboxAsync(CancellationToken ct)
    {
        if (Settings.hitboxActivateDelay > 0f)
        {
            bool canceled = await UniTask.WaitForSeconds(Settings.hitboxActivateDelay, cancellationToken: ct).SuppressCancellationThrow();
            if (canceled) return;
        }
        _hitbox.enabled = true;
        await UniTask.WaitForSeconds(Settings.hitboxActiveDuration, cancellationToken: ct).SuppressCancellationThrow();
        if (_hitbox != null) _hitbox.enabled = false;
    }

    private async UniTaskVoid WaitThenDespawnAsync(CancellationToken ct)
    {
#if UNITY_VFX_GRAPH
        if (_vfx != null)
        {
            bool canceled = await UniTask.Yield(PlayerLoopTiming.Update, ct).SuppressCancellationThrow();
            if (canceled) return;
        }
#endif
        bool waitCanceled = await UniTask.WaitUntil(AllEffectsDone, cancellationToken: ct).SuppressCancellationThrow();
        if (waitCanceled) return;
        ReturnToPool();
    }

    private bool AllEffectsDone()
    {
        if (_ps != null && _ps.IsAlive(true)) return false;
#if UNITY_VFX_GRAPH
        if (_vfx != null && _vfx.HasAnySystemAwake()) return false;
#endif
        return true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!Settings.dealsDamage) return;
        if ((Settings.hitboxLayers.value & (1 << other.gameObject.layer)) == 0) return;
        if (other.TryGetComponent<IDamageable>(out var damageable))
            damageable.TakeDamage(_currentDamage);
    }
}
