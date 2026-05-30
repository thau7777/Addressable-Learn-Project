using System.Threading;
using Cysharp.Threading.Tasks;
using LokiInspector;
using UnityEngine;

public class HitFlash : MonoBehaviour
{
    [TabGroup("Refs"), SerializeField] private Renderer _targetRenderer;
    private IDamageable _owner;

    [TabGroup("Flash"), LabelText("Peak Intensity"), SerializeField] private float _peakIntensity = 3.5f;
    [TabGroup("Flash"), LabelText("Duration (s)"), SerializeField] private float _duration = 0.1f;

    private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

    private MaterialPropertyBlock _mpb;
    private CancellationTokenSource _flashCts;

    private void Awake()
    {
        _mpb = new MaterialPropertyBlock();
    }
    private void OnDestroy()
    {
        if (_owner != null) _owner.OnDamaged -= HandleDamaged;
    }

    private void OnDisable()
    {
        CancelFlash();
        ClearEmission();
    }

    public void SubscribeOnDamaged()
    {
        _owner = GetComponent<IDamageable>();
        if (_owner == null)
        {
            Debug.LogError($"[HitFlash] {name} has no IDamageable on this GameObject — flash disabled.", this);
            enabled = false;
            return;
        }
        _owner.OnDamaged += HandleDamaged;
    }

    private void HandleDamaged(float _) => FlashAsync().Forget();

    private async UniTaskVoid FlashAsync()
    {
        CancelFlash();
        _flashCts = new CancellationTokenSource();
        var ct = _flashCts.Token;

        float elapsed = 0f;
        while (elapsed < _duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _duration);
            float intensity = (1f - t * t) * _peakIntensity;
            SetEmission(Color.white * intensity);
            bool canceled = await UniTask.Yield(PlayerLoopTiming.Update, ct).SuppressCancellationThrow();
            if (canceled) return;
        }
        ClearEmission();
    }

    private void CancelFlash()
    {
        if (_flashCts == null) return;
        _flashCts.Cancel();
        _flashCts.Dispose();
        _flashCts = null;
    }

    private void SetEmission(Color color)
    {
        if (_targetRenderer == null) return;
        _targetRenderer.GetPropertyBlock(_mpb);
        _mpb.SetColor(EmissionColorID, color);
        _targetRenderer.SetPropertyBlock(_mpb);
    }

    private void ClearEmission() => SetEmission(Color.black);
}
