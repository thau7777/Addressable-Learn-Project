using System.Collections;
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
    private Coroutine _despawnRoutine;
    private Coroutine _hitboxRoutine;

    private void Awake()
    {
        _ps = GetComponentInChildren<ParticleSystem>();
#if UNITY_VFX_GRAPH
        _vfx = GetComponentInChildren<VisualEffect>();
#endif
        _hitbox = GetComponent<Collider>();
        if (_hitbox != null) _hitbox.enabled = false;
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

        if (Settings.dealsDamage && _hitbox != null)
            _hitboxRoutine = StartCoroutine(HitboxRoutine());

        _despawnRoutine = StartCoroutine(WaitThenDespawn());
    }

    private void OnDisable()
    {
        if (_hitboxRoutine != null) { StopCoroutine(_hitboxRoutine); _hitboxRoutine = null; }
        if (_despawnRoutine != null) { StopCoroutine(_despawnRoutine); _despawnRoutine = null; }
        if (_hitbox != null) _hitbox.enabled = false;

        if (_ps != null) _ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
#if UNITY_VFX_GRAPH
        if (_vfx != null) _vfx.Stop();
#endif
    }

    private IEnumerator HitboxRoutine()
    {
        if (Settings.hitboxActivateDelay > 0f)
            yield return new WaitForSeconds(Settings.hitboxActivateDelay);
        _hitbox.enabled = true;
        yield return new WaitForSeconds(Settings.hitboxActiveDuration);
        _hitbox.enabled = false;
    }

    private IEnumerator WaitThenDespawn()
    {
#if UNITY_VFX_GRAPH
        if (_vfx != null) yield return null; // one frame for VFX to initialize
#endif
        yield return new WaitUntil(AllEffectsDone);
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
