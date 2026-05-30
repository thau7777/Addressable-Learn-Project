using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "AddWeaponBuff", menuName = "Scriptable Objects/Buffs/Weapon/Add Weapon")]
public class AddWeaponBuff : BuffSO
{
    [SerializeField] private WeaponData _weaponData;

    public override async void Apply()
    {
        await ApplyAsync();
        base.Apply();
    }
    public override string GetRuntimeDescription()
    {
        throw new System.NotImplementedException();
    }
    public override void Remove()
    {
        if (_weaponData == null) return;
        EventBus<RemoveWeaponEvent>.Raise(new RemoveWeaponEvent(_weaponData));
    }

    private async UniTask ApplyAsync()
    {
        await _weaponData.LoadWeaponAssetsAsync();
        EventBus<AddWeaponEvent>.Raise(new AddWeaponEvent(_weaponData));
    }
}
