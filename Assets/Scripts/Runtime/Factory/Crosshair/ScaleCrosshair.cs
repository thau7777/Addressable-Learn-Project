using UnityEngine;
using UnityEngine.UI;
using PrimeTween;

public class ScaleCrosshair : Crosshair
{
    [SerializeField] private Image _dotImage;
    [SerializeField] private float _targetScale;
    [SerializeField] private float _scaleBigLerpDuration = 0.1f;
    [SerializeField] private float _scaleSmallLerpDuration = 0.1f;
    [SerializeField] private AnimationCurve _scaleOutCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve _scaleInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Vector3 _originalScale;
    private Tween _tween;

    private void Awake()
    {
        _dotImage = GetComponentInChildren<Image>();
        _originalScale = _dotImage.rectTransform.localScale;
    }

    public override void OnExecute()
    {
        _tween.Stop(); // interrupts mid-punch cleanly

        var rect = _dotImage.rectTransform;
        Vector3 bigScale = _originalScale * _targetScale;

        _tween = Tween.Scale(rect, bigScale, _scaleBigLerpDuration, _scaleOutCurve)
            .OnComplete(() =>
            {
                _tween = Tween.Scale(rect, _originalScale, _scaleSmallLerpDuration, _scaleInCurve);
            });
    }
    public override void ResetSpread()
    {
        _tween.Stop();
        _dotImage.rectTransform.localScale = _originalScale;
    }
}