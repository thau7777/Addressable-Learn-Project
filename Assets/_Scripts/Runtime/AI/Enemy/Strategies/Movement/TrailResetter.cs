using UnityEngine;

[RequireComponent(typeof(TrailRenderer))]
public class TrailResetter : MonoBehaviour
{
    private TrailRenderer _trail;

    private void Awake() => _trail = GetComponent<TrailRenderer>();

    private void OnDisable()
    {
        if (_trail == null) return;
        _trail.emitting = false;
        _trail.Clear();
        _trail.enabled = false;
    }

    public void Activate()
    {
        if (_trail == null) return;
        _trail.Clear();
        _trail.emitting = true;
        _trail.enabled = true;
    }

    public void Deactivate()
    {
        if (_trail == null) return;
        _trail.emitting = false;
        _trail.Clear();
        _trail.enabled = false;
    }
}
