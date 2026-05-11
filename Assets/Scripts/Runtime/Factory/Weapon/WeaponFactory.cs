using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class WeaponFactory : Singleton<WeaponFactory>
{
    public IWeapon GetWeapon(WeaponData data)
    {
        if (!data.WeaponPrefab) return null;
        Weapon newWeapon = Instantiate(data.WeaponPrefab).GetComponent<Weapon>();

        newWeapon.SetWeaponData(data);
        return newWeapon;
    }

    //public void ReleaseAllWeaponAssets()
    //{
    //    foreach (var kvp in _weaponCache)
    //    {
    //        Addressables.ReleaseInstance(kvp.Value.gameObject);
    //    }

    //    _weaponCache.Clear();
    //    _currentWeapon = null;
    //}

}