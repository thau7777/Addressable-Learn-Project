using LokiInspector;
using UnityEngine;

public class ExpManager : Singleton<ExpManager>
{
    [TabGroup("Curve")]
    [SerializeField] private float _baseExpNeeded = 100f;

    [TabGroup("Curve")]
    [SerializeField] private float _expMultiplierPerLevel = 1.5f;

    private float _currentExp;
    private float _expNeeded;
    private int _currentLevel = 1;

    public int CurrentLevel => _currentLevel;
    public float Progress01 => _expNeeded > 0f ? _currentExp / _expNeeded : 0f;

    private EventBinding<ExpPickupEvent> _expPickupBinding;

    protected override void Awake()
    {
        base.Awake();
        _expNeeded = _baseExpNeeded;
    }

    private void OnEnable()
    {
        _expPickupBinding = new EventBinding<ExpPickupEvent>(OnExpPickedUp);
        EventBus<ExpPickupEvent>.Register(_expPickupBinding);
    }

    private void OnDisable()
    {
        EventBus<ExpPickupEvent>.Deregister(_expPickupBinding);
    }

    [Button]
    public void TestExpPickup() => OnExpPickedUp(new ExpPickupEvent(15));

    private void OnExpPickedUp(ExpPickupEvent evtData)
    {
        _currentExp += evtData.ExpAmount;
        while (_currentExp >= _expNeeded)
        {
            _currentLevel++;
            _currentExp -= _expNeeded;
            _expNeeded *= _expMultiplierPerLevel;
            EventBus<LevelUpEvent>.Raise(new LevelUpEvent(_currentLevel));
        }
        EventBus<ExpProgressChangedEvent>.Raise(new ExpProgressChangedEvent(_currentExp / _expNeeded));
    }
}
