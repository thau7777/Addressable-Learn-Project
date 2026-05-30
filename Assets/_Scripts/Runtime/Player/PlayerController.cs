using LokiInspector;
using UnityEngine;

public class PlayerController : Singleton<PlayerController>
{
    [SerializeField, TabGroup("References")] private InputReader _inputReader;

    public Rigidbody Rb { get; private set; }
    public float MoveSpeed => _characterStats.MoveSpeed;
    public float MaxHealth => _characterStats.MaxHealth;
    public float CurrentHealth => _characterStats.CurrentHealth;
    public float PickupRadius => _characterStats.PickupRadius;
    public Vector3 InputDir { get; private set; }
    public Vector3 PendingKnockback { get; private set; }
    public StateMachine SM { get; private set; }

    private CharacterStats _characterStats;

    private EventBinding<ApplyCharacterStatsModifierEvent> _applyStatsBinding;
    private EventBinding<RemoveCharacterStatsModifierEvent> _removeStatsBinding;

    #region Initialization
    protected override void Awake()
    {
        base.Awake();
        Rb = GetComponent<Rigidbody>();
    }

    public void Initialize(CharacterInfos infos)
    {
        _characterStats = new CharacterStats(infos);
        _characterStats.Activate();
        StatesInit();
    }

    private void OnDestroy()
    {
        _characterStats?.Deactivate();
    }

    private void OnEnable()
    {
        _inputReader.gameplayActions.onMove += OnMove;

        _applyStatsBinding = new EventBinding<ApplyCharacterStatsModifierEvent>(OnApplyStatsModifier);
        EventBus<ApplyCharacterStatsModifierEvent>.Register(_applyStatsBinding);

        _removeStatsBinding = new EventBinding<RemoveCharacterStatsModifierEvent>(OnRemoveStatsModifier);
        EventBus<RemoveCharacterStatsModifierEvent>.Register(_removeStatsBinding);
    }

    private void OnDisable()
    {
        _inputReader.gameplayActions.onMove -= OnMove;
        EventBus<ApplyCharacterStatsModifierEvent>.Deregister(_applyStatsBinding);
        EventBus<RemoveCharacterStatsModifierEvent>.Deregister(_removeStatsBinding);
    }

    private void StatesInit()
    {
        SM = new StateMachine();
        SM.RegisterState(new PlayerIdle(this));
        SM.RegisterState(new PlayerMove(this));
        SM.RegisterState(new PlayerHurt(this));
        SM.Init<PlayerIdle>();
    }
    #endregion

    #region Loops
    private void Update()
    {
        if (SM == null) return;
        SM.Tick();
    }

    private void FixedUpdate()
    {
        if (SM == null) return;
        SM.FixedTick();
    }
    #endregion

    #region Inputs
    private void OnMove(Vector2 dir)
    {
        InputDir = new Vector3(dir.x, 0f, dir.y).normalized;
    }
    #endregion

    public void ApplyKnockback(Vector3 force)
    {
        PendingKnockback = force;
        SM.ForceTransition<PlayerHurt>();
    }

    #region Stats Modifiers
    private void OnApplyStatsModifier(ApplyCharacterStatsModifierEvent evt)
    {
        var m = evt.Modifier;
        if (m == null || _characterStats == null) return;

        var mv = m.Movement;
        var me = m.Melee;
        var rg = m.Ranged;

        _characterStats.MoveSpeed     *= 1f + mv.moveSpeedPercent     * 0.01f;
        _characterStats.PickupRadius  *= 1f + mv.pickupRadiusPercent  * 0.01f;

        float prevMax = _characterStats.MaxHealth;
        _characterStats.MaxHealth *= 1f + mv.maxHealthPercent * 0.01f;
        _characterStats.CurrentHealth = Mathf.Clamp(
            _characterStats.CurrentHealth + (_characterStats.MaxHealth - prevMax),
            0f, _characterStats.MaxHealth);

        _characterStats.MeleeDamage     *= 1f + me.damagePercent     * 0.01f;
        _characterStats.MeleeAttackRate *= 1f + me.attackRatePercent * 0.01f;
        _characterStats.MeleeRangeMul   *= 1f + me.rangePercent      * 0.01f;

        _characterStats.RangedDamage     *= 1f + rg.damagePercent     * 0.01f;
        _characterStats.RangedAttackRate *= 1f + rg.attackRatePercent * 0.01f;
        _characterStats.RangedRangeMul   *= 1f + rg.rangePercent      * 0.01f;
    }

    private void OnRemoveStatsModifier(RemoveCharacterStatsModifierEvent evt)
    {
        var m = evt.Modifier;
        if (m == null || _characterStats == null) return;

        var mv = m.Movement;
        var me = m.Melee;
        var rg = m.Ranged;

        _characterStats.MoveSpeed     /= 1f + mv.moveSpeedPercent     * 0.01f;
        _characterStats.PickupRadius  /= 1f + mv.pickupRadiusPercent  * 0.01f;

        float prevMax = _characterStats.MaxHealth;
        _characterStats.MaxHealth /= 1f + mv.maxHealthPercent * 0.01f;
        _characterStats.CurrentHealth = Mathf.Clamp(
            _characterStats.CurrentHealth - (prevMax - _characterStats.MaxHealth),
            0f, _characterStats.MaxHealth);

        _characterStats.MeleeDamage     /= 1f + me.damagePercent     * 0.01f;
        _characterStats.MeleeAttackRate /= 1f + me.attackRatePercent * 0.01f;
        _characterStats.MeleeRangeMul   /= 1f + me.rangePercent      * 0.01f;

        _characterStats.RangedDamage     /= 1f + rg.damagePercent     * 0.01f;
        _characterStats.RangedAttackRate /= 1f + rg.attackRatePercent * 0.01f;
        _characterStats.RangedRangeMul   /= 1f + rg.rangePercent      * 0.01f;
    }
    #endregion

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent<Pickable>(out var pickable))
        {
            pickable.Attract(transform);
        }
    }
}
