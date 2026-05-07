using UnityEngine;
using PrimeTween;

public class SpreadableCrosshair : Crosshair
{
    private GunData GunData => _weaponData as GunData;
    [SerializeField] private RectTransform[] corners;

    private Vector2[] _origins;
    private Tween[] _tweens;

    private void Awake()
    {
        int n = corners.Length;
        _origins = new Vector2[n];
        _tweens = new Tween[n];

        for (int i = 0; i < n; i++)
            _origins[i] = corners[i].anchoredPosition;
    }

    public override void OnExecute()
    {
        for (int i = 0; i < corners.Length; i++)
        {
            _tweens[i].Stop(); // safe even if already idle; corner stays at current pos

            Vector2 currentPos = corners[i].anchoredPosition;
            float dist = Vector2.Distance(currentPos, _origins[i]);
            float excess = Mathf.Max(0f, dist - GunData.MaxSpreadThreshold);
            float scaledOffset = GunData.SpreadOnShoot / (1f + excess);
            Vector2 spreadTarget = currentPos + _origins[i].normalized * scaledOffset;

            int captured = i; // capture for closure
            _tweens[i] = Tween.UIAnchoredPosition(corners[i], spreadTarget, GunData.SpreadDuration, GunData.SpreadCurve)
                .OnComplete(() => StartReturn(captured));
        }
    }

    private void StartReturn(int i)
    {
        float dist = Vector2.Distance(corners[i].anchoredPosition, _origins[i]);
        float returnDuration = dist * GunData.ReturnDuration;

        _tweens[i] = Tween.UIAnchoredPosition(corners[i], _origins[i], returnDuration, GunData.ReturnCurve);
    }

    public override void ResetSpread()
    {
        for (int i = 0; i < corners.Length; i++)
        {
            _tweens[i].Stop();
            corners[i].anchoredPosition = _origins[i];
        }
    }
}