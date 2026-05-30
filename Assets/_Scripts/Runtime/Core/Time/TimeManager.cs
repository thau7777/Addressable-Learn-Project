using System.Collections.Generic;
using LokiInspector;
using UnityEngine;

public class TimeManager : PersistentSingleton<TimeManager>
{
    [TabGroup("Debug")]
    [SerializeField, ReadOnly, LabelText("Current Scale")]
    private float _currentScale = 1f;

    [TabGroup("Debug")]
    [SerializeField, ReadOnly, LabelText("Stack Depth")]
    private int _activeCount;

    [TabGroup("Debug")]
    [SerializeField, ReadOnly, LabelText("Active Key")]
    private TimeModifier _activeKey;

    [TabGroup("Debug")]
    [SerializeField, ReadOnly, LabelText("Active Remaining")]
    private float _activeRemaining;

    private readonly List<ModifierEntry> _stack = new();
    private int _activeIndex = -1;

    public float CurrentScale => _currentScale;
    public float DeltaTime => Time.deltaTime;
    public float UnscaledDeltaTime => Time.unscaledDeltaTime;

    protected override void Awake()
    {
        base.Awake();
        ApplyScale(1f);
    }

    private void OnDestroy()
    {
        if (instance == this)
            Time.timeScale = 1f;
    }

    public void PushScale(TimeModifier key, float scale)
    {
        UpsertEntry(key, scale, float.PositiveInfinity);
        Recalculate();
    }

    public void PushScaleFor(TimeModifier key, float scale, float realSeconds)
    {
        UpsertEntry(key, scale, Mathf.Max(0f, realSeconds));
        Recalculate();
    }

    public void PopScale(TimeModifier key)
    {
        int idx = FindIndexByKey(key);
        if (idx < 0) return;
        _stack.RemoveAt(idx);
        Recalculate();
    }

    [Button]
    public void ClearAll()
    {
        if (_stack.Count == 0) return;
        _stack.Clear();
        Recalculate();
    }

    private void Update()
    {
        if (_activeIndex < 0) return;

        var entry = _stack[_activeIndex];
        if (float.IsPositiveInfinity(entry.RemainingDuration)) return;

        entry.RemainingDuration -= Time.unscaledDeltaTime;
        if (entry.RemainingDuration <= 0f)
        {
            _stack.RemoveAt(_activeIndex);
            Recalculate();
        }
        else
        {
            _stack[_activeIndex] = entry;
            _activeRemaining = entry.RemainingDuration;
        }
    }

    private void UpsertEntry(TimeModifier key, float scale, float remaining)
    {
        float clamped = Mathf.Clamp(scale, 0f, 1f);
        int idx = FindIndexByKey(key);
        var entry = new ModifierEntry(key, clamped, remaining);
        if (idx >= 0) _stack[idx] = entry;
        else _stack.Add(entry);
    }

    private int FindIndexByKey(TimeModifier key)
    {
        for (int i = 0; i < _stack.Count; i++)
            if (_stack[i].Key == key) return i;
        return -1;
    }

    private void Recalculate()
    {
        _activeIndex = -1;
        float min = float.PositiveInfinity;
        for (int i = 0; i < _stack.Count; i++)
        {
            if (_stack[i].Scale < min)
            {
                min = _stack[i].Scale;
                _activeIndex = i;
            }
        }

        _activeCount = _stack.Count;
        if (_activeIndex >= 0)
        {
            _activeKey = _stack[_activeIndex].Key;
            _activeRemaining = _stack[_activeIndex].RemainingDuration;
            ApplyScale(min);
        }
        else
        {
            _activeRemaining = 0f;
            ApplyScale(1f);
        }
    }

    private void ApplyScale(float scale)
    {
        if (Mathf.Approximately(_currentScale, scale) &&
            Mathf.Approximately(Time.timeScale, scale))
            return;

        _currentScale = scale;
        Time.timeScale = scale;
        EventBus<TimeScaleChangedEvent>.Raise(new TimeScaleChangedEvent(scale));
    }

    private struct ModifierEntry
    {
        public TimeModifier Key;
        public float Scale;
        public float RemainingDuration;

        public ModifierEntry(TimeModifier key, float scale, float remaining)
        {
            Key = key;
            Scale = scale;
            RemainingDuration = remaining;
        }
    }
}
