using Cysharp.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "EquipWeaponBuff", menuName = "Scriptable Objects/Buffs/Weapon/Equip Weapon")]
public class EquipWeaponBuffSO : BuffSO
{
    [SerializeField] private WeaponData _weaponData;

    public override void Apply() => ApplyAsync().Forget();

    public override void Remove() => WeaponManager.Instance.UnequipWeapon(_weaponData);

    private async UniTaskVoid ApplyAsync()
    {
        await _weaponData.LoadWeaponAssetsAsync();
        WeaponManager.Instance.EquipWeapon(_weaponData);
    }
}
