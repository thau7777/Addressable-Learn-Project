using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using LokiInspector;
using UnityEngine;

public class BuffUIController : MonoBehaviour
{
    [TabGroup("Refs")]
    [SerializeField] private RectTransform _buffContainer;

    [TabGroup("Refs")]
    [SerializeField] private CanvasGroup _canvasGroup;

    [TabGroup("Refs")]
    [SerializeField] private BuffCard[] _buffCards = new BuffCard[3];

    [TabGroup("Animation")]
    [SerializeField] private float _slideOffsetY = -400f;

    [TabGroup("Animation")]
    [SerializeField] private float _slideDuration = 0.35f;

    [TabGroup("Animation")]
    [SerializeField] private AnimationCurve _slideCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [TabGroup("Animation")]
    [SerializeField] private float _fadeDuration = 0.35f;

    [TabGroup("Animation")]
    [SerializeField] private AnimationCurve _fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private EventBinding<ShowBuffsEvent> _showBuffsBinding;
    private CancellationTokenSource _animCts;
    private Vector2 _shownAnchoredPos;

    private void Awake()
    {
        if (_buffContainer != null)
            _shownAnchoredPos = _buffContainer.anchoredPosition;
    }

    private void OnEnable()
    {
        _showBuffsBinding = new EventBinding<ShowBuffsEvent>(OnShowBuffs);
        EventBus<ShowBuffsEvent>.Register(_showBuffsBinding);
        HideImmediate();
    }

    private void OnDisable()
    {
        EventBus<ShowBuffsEvent>.Deregister(_showBuffsBinding);
        _animCts?.Cancel();
        _animCts?.Dispose();
        _animCts = null;
    }

    private void OnShowBuffs(ShowBuffsEvent evt)
    {
        var buffsArray = evt.BuffsArray;
        int count = buffsArray?.Length ?? 0;

        for (int i = 0; i < _buffCards.Length; i++)
        {
            var card = _buffCards[i];
            if (card == null) continue;

            if (i < count)
            {
                card.gameObject.SetActive(true);
                card.Bind(buffsArray[i], OnBuffSelected);
            }
            else
            {
                card.gameObject.SetActive(false);
            }
        }

        Show().Forget();
    }

    private void OnBuffSelected(BuffSO buff)
    {
        BuffManager.Instance.ApplyBuff(buff);
        Hide().Forget();
    }

    private void HideImmediate()
    {
        if (_buffContainer == null) return;
        _buffContainer.anchoredPosition = _shownAnchoredPos + new Vector2(0f, _slideOffsetY);
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }
        _buffContainer.gameObject.SetActive(false);
    }

    private async UniTaskVoid Show()
    {
        if (_buffContainer == null) return;

        _buffContainer.gameObject.SetActive(true);
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }

        var token = ResetCts();
        var from = _shownAnchoredPos + new Vector2(0f, _slideOffsetY);
        _buffContainer.anchoredPosition = from;

        await UniTask.WhenAll(
            Slide(from, _shownAnchoredPos, token),
            Fade(0f, 1f, token));
    }

    private async UniTaskVoid Hide()
    {
        EventBus<HideBuffsEvent>.Raise(new HideBuffsEvent());
        if (_buffContainer == null) return;
        if (_canvasGroup != null)
        {
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        var token = ResetCts();
        float startAlpha = _canvasGroup != null ? _canvasGroup.alpha : 1f;
        await Fade(startAlpha, 0f, token);

        if (token.IsCancellationRequested) return;
        _buffContainer.gameObject.SetActive(false);
        TimeManager.Instance.PopScale(TimeModifier.Pause);
    }

    private CancellationToken ResetCts()
    {
        _animCts?.Cancel();
        _animCts?.Dispose();
        _animCts = new CancellationTokenSource();
        return _animCts.Token;
    }

    private async UniTask Slide(Vector2 from, Vector2 to, CancellationToken token)
    {
        float elapsed = 0f;
        while (elapsed < _slideDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / _slideDuration);
            float eased = _slideCurve.Evaluate(t);
            _buffContainer.anchoredPosition = Vector2.LerpUnclamped(from, to, eased);

            try
            {
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
        _buffContainer.anchoredPosition = to;
    }

    private async UniTask Fade(float from, float to, CancellationToken token)
    {
        if (_canvasGroup == null) return;

        float elapsed = 0f;
        _canvasGroup.alpha = from;

        while (elapsed < _fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / _fadeDuration);
            float eased = _fadeCurve.Evaluate(t);
            _canvasGroup.alpha = Mathf.LerpUnclamped(from, to, eased);

            try
            {
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
        _canvasGroup.alpha = to;
    }
}
