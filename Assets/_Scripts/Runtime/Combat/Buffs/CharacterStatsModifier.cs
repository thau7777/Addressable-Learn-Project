using System;
using System.Text;
using LokiInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterStatsModifier", menuName = "Scriptable Objects/Buffs/CharacterStatsModifier")]
public class CharacterStatsModifier : BuffSO
{
    [Serializable]
    public struct MovementStatBlock
    {
        [Range(-100f, 100f)] public float moveSpeedPercent;
        [Range(-100f, 100f)] public float maxHealthPercent;
        [Range(-100f, 100f)] public float pickupRadiusPercent;
    }

    [Serializable]
    public struct CombatStatBlock
    {
        [Range(-100f, 100f)] public float damagePercent;
        [Range(-100f, 100f)] public float attackRatePercent;
        [Range(-100f, 100f)] public float rangePercent;
    }

    [TabGroup("Movement"), SerializeField] private MovementStatBlock _movement;
    [TabGroup("Melee"),    SerializeField] private CombatStatBlock _melee;
    [TabGroup("Ranged"),   SerializeField] private CombatStatBlock _ranged;

    public MovementStatBlock Movement => _movement;
    public CombatStatBlock Melee => _melee;
    public CombatStatBlock Ranged => _ranged;

    public override void Apply()
    {
        EventBus<ApplyCharacterStatsModifierEvent>.Raise(new ApplyCharacterStatsModifierEvent(this));
        base.Apply();
    }

    public override void Remove()
        => EventBus<RemoveCharacterStatsModifierEvent>.Raise(new RemoveCharacterStatsModifierEvent(this));

    public override string GetRuntimeDescription()
    {
        var sb = new StringBuilder();

        var stats = CharacterStats.Current;

        if(stats == null)
        {
            sb.AppendLine("\n\n<color=yellow><i>Base stats unknown</i></color>");
            return sb.ToString();
        }
        if(!Mathf.Approximately(_movement.moveSpeedPercent,0))
            AppendStatLine(sb, "Move Speed",     stats.MoveSpeed, _movement.moveSpeedPercent);
        if(!Mathf.Approximately(_movement.maxHealthPercent,0))
            AppendStatLine(sb, "Max Health",     stats.MaxHealth , _movement.maxHealthPercent);
        if(!Mathf.Approximately(_movement.pickupRadiusPercent,0))
            AppendStatLine(sb, "Pickup Radius",  stats.PickupRadius  , _movement.pickupRadiusPercent);
        if(!Mathf.Approximately(_melee.damagePercent,0))
            AppendStatLine(sb, "Melee Damage",       stats.MeleeDamage      , _melee.damagePercent);
        if(!Mathf.Approximately(_melee.attackRatePercent,0))
            AppendStatLine(sb, "Melee AR",  stats.MeleeAttackRate  , _melee.attackRatePercent);
        if(!Mathf.Approximately(_melee.rangePercent,0))
            AppendStatLine(sb, "Melee Range",        stats.MeleeRangeMul    , _melee.rangePercent);
        if(!Mathf.Approximately(_ranged.damagePercent,0))
            AppendStatLine(sb, "Ranged Damage",      stats.RangedDamage     , _ranged.damagePercent);
        if(!Mathf.Approximately(_ranged.attackRatePercent,0))
            AppendStatLine(sb, "Ranged AR", stats.RangedAttackRate , _ranged.attackRatePercent);
        if(!Mathf.Approximately(_ranged.rangePercent,0))
            AppendStatLine(sb, "Ranged Range",       stats.RangedRangeMul   , _ranged.rangePercent);

        return sb.ToString();
    }
}
