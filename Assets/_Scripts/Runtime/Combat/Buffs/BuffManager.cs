using System.Collections.Generic;
using LokiInspector;
using UnityEngine;

public class BuffManager : Singleton<BuffManager>
{
    [TabGroup("Buff Pool")]
    [SerializeField] private BuffSO[] _allBuffs;

    [TabGroup("Buff Pool")]
    [SerializeField] private int _choicesPerLevelUp = 3;

    [TabGroup("Buff Pool")]
    [SerializeField, Range(0f, 1f)]
    [LabelText("Active Buff Weight Boost")]
    private float _activeBuffBoost = 0.15f;


    private readonly Dictionary<BuffSO, int> _activeBuffs = new();
    private readonly HashSet<BuffSO> _activeSetCache = new();
    private readonly BuffSelector _selector = new();

    private EventBinding<LevelUpEvent> _levelUpBinding;

    private void OnEnable()
    {
        _levelUpBinding = new EventBinding<LevelUpEvent>(OnLevelUp);
        EventBus<LevelUpEvent>.Register(_levelUpBinding);
    }

    private void OnDisable()
    {
        EventBus<LevelUpEvent>.Deregister(_levelUpBinding);
    }
    private void OnLevelUp(LevelUpEvent _)
    {
        _activeSetCache.Clear();
        foreach (var key in _activeBuffs.Keys) _activeSetCache.Add(key);

        var choices = _selector.Pick(_allBuffs, _activeSetCache, _activeBuffBoost, _choicesPerLevelUp);
        EventBus<ShowBuffsEvent>.Raise(new ShowBuffsEvent(choices));
        TimeManager.Instance.PushScale(TimeModifier.Pause,0f);
    }
    public void ApplyBuff(BuffSO buff)
    {
        if (buff == null) return;
        buff.Apply();

        if(_activeBuffs.ContainsKey(buff))
            _activeBuffs[buff]++;
        else
            _activeBuffs[buff] = 1;
    }

    public void RemoveBuff(BuffSO buff)
    {
        if (buff == null) return;
        if (_activeBuffs[buff]-- == 0)
            buff.Remove();
    }
}
