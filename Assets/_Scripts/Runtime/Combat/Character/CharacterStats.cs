public class CharacterStats
{
    public static CharacterStats Current { get; private set; }

    public float MoveSpeed;
    public float MaxHealth;
    public float CurrentHealth;
    public float PickupRadius;

    public float MeleeDamage;
    public float RangedDamage;
    public float MeleeAttackRate;
    public float RangedAttackRate;
    public float MeleeRangeMul;
    public float RangedRangeMul;

    private readonly CharacterInfos _infos;

    public CharacterStats(CharacterInfos infos)
    {
        _infos = infos;
        ResetToBase();
    }

    public void Activate() => Current = this;

    public void Deactivate()
    {
        if (Current == this) Current = null;
    }

    public void ResetToBase()
    {
        MoveSpeed = _infos.BaseMoveSpeed;
        MaxHealth = _infos.BaseMaxHealth;
        CurrentHealth = _infos.BaseMaxHealth;
        PickupRadius = _infos.BasePickupRadius;

        MeleeDamage = _infos.BaseMeleeDamage;
        RangedDamage = _infos.BaseRangedDamage;
        MeleeAttackRate = _infos.BaseMeleeAttackRate;
        RangedAttackRate = _infos.BaseRangedAttackRate;
        MeleeRangeMul = _infos.BaseMeleeRangeMul;
        RangedRangeMul = _infos.BaseRangedRangeMul;
    }
}
