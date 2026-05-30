public struct ShowBuffsEvent : IEvent
{
    public BuffSO[] BuffsArray { get; }
    public ShowBuffsEvent(BuffSO[] buffsArray)
    {
        BuffsArray = buffsArray;
    }
}

public struct HideBuffsEvent : IEvent
{
}

public struct AddWeaponEvent : IEvent
{
    public WeaponData WeaponData { get; }
    public AddWeaponEvent(WeaponData weaponData)
    {
        WeaponData = weaponData;
    }
}

public struct RemoveWeaponEvent : IEvent
{
    public WeaponData WeaponData { get; }
    public RemoveWeaponEvent(WeaponData weaponData)
    {
        WeaponData = weaponData;
    }
}

public struct ApplyCharacterStatsModifierEvent : IEvent
{
    public CharacterStatsModifier Modifier { get; }
    public ApplyCharacterStatsModifierEvent(CharacterStatsModifier modifier)
    {
        Modifier = modifier;
    }
}

public struct RemoveCharacterStatsModifierEvent : IEvent
{
    public CharacterStatsModifier Modifier { get; }
    public RemoveCharacterStatsModifierEvent(CharacterStatsModifier modifier)
    {
        Modifier = modifier;
    }
}
