using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;

public class WeaponFactory : Singleton<WeaponFactory>
{
    private Dictionary<AssetReference, Weapon> _weaponCache = new Dictionary<AssetReference, Weapon>();
    private Weapon _currentWeapon;

    public IWeapon GetWeapon(WeaponData data)
    {
        DisableCurrentWeapon();
        if (!data.WeaponPrefab) return null;

        if (_weaponCache.TryGetValue(data.weaponPrefabRef, out Weapon cachedWeapon))
        {
            cachedWeapon.gameObject.SetActive(true);
            _currentWeapon = cachedWeapon;
        }
        else
        {
            _currentWeapon = Instantiate(data.WeaponPrefab).GetComponent<Weapon>();
            _weaponCache[data.weaponPrefabRef] = _currentWeapon;
            _currentWeapon.IsInitialized = false;
            data.OnWeaponFirstCreate();
        }

        _currentWeapon.SetWeaponData(data);
        return _currentWeapon;
    }

    public void DisableCurrentWeapon()
    {
        if (_currentWeapon == null) return;
        _currentWeapon = null;
    }

    public void ReleaseAllWeaponAssets()
    {
        foreach (var kvp in _weaponCache)
        {
            kvp.Value.WeaponData.StopTask();
            Addressables.ReleaseInstance(kvp.Value.gameObject);
        }

        _weaponCache.Clear();
        _currentWeapon = null;
    }

    private void OnDisable()
    {
        ReleaseAllWeaponAssets();
    }
}