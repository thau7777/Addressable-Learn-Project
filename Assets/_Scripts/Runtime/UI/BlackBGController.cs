using Cysharp.Threading.Tasks;
using LokiInspector;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class BlackBGController : MonoBehaviour
{
    [TabGroup("BG Panel")]
    [Required]
    [SerializeField]
    private Image _blackBGPanel;

    [TabGroup("BG Panel")]
    [SerializeField]
    private float _lerpDuration = 0.25f;

    private CancellationTokenSource _lerpCts;
    private EventBinding<ShowBuffsEvent> _showBuffsBinding;
    private EventBinding<HideBuffsEvent> _hideBuffsBinding;
    private void OnEnable()
    {
        _showBuffsBinding = new EventBinding<ShowBuffsEvent>(LerpIn);
        EventBus<ShowBuffsEvent>.Register(_showBuffsBinding);

        _hideBuffsBinding = new EventBinding<HideBuffsEvent>(LerpOut);
        EventBus<HideBuffsEvent>.Register(_hideBuffsBinding);

        _blackBGPanel.gameObject.SetActive(false);
    }


    private void OnDisable()
    {
        EventBus<ShowBuffsEvent>.Deregister(_showBuffsBinding);
        EventBus<HideBuffsEvent>.Deregister(_hideBuffsBinding);
        _lerpCts?.Cancel();
        _lerpCts?.Dispose();
        _lerpCts = null;
    }

    public void LerpIn() => LerpBGPanel(0.9f).Forget();

    public void LerpOut() => LerpBGPanel(0f).Forget();

    private async UniTaskVoid LerpBGPanel(float target)
    {
        if(target > 0f)
            _blackBGPanel.gameObject.SetActive(true);
        _lerpCts?.Cancel();
        _lerpCts?.Dispose();
        _lerpCts = new CancellationTokenSource();
        var token = _lerpCts.Token;

        Color color = _blackBGPanel.color;
        float startAlpha = color.a;
        float elapsed = 0f;

        while (elapsed < _lerpDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / _lerpDuration);
            color.a = Mathf.Lerp(startAlpha, target, t);
            _blackBGPanel.color = color;

            try
            {
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }

        color.a = target;
        _blackBGPanel.color = color;
        if(target == 0)
            _blackBGPanel.gameObject.SetActive(false);
    }
}
