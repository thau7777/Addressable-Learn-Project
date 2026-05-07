using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class CrosshairFactory : Singleton<CrosshairFactory>
{
    [SerializeField] private Canvas _crosshairCanvas;
    public AssetReference defaultCrosshairPrefabRef;
    private Dictionary<AssetReference, Crosshair> _crosshairCache = new Dictionary<AssetReference, Crosshair>();
    private Crosshair _currentCrosshair;

    public ICrosshair GetCrosshair(WeaponData weaponData)
    {
        if (_crosshairCache.TryGetValue(weaponData.weaponCrosshairRef, out Crosshair cachedCrosshair))
        {
            if (cachedCrosshair == _currentCrosshair)
            {
                _currentCrosshair.SetWeaponData(weaponData);
                return _currentCrosshair;
            }
            DisableCurrentCrosshair();
            cachedCrosshair.gameObject.SetActive(true);
            cachedCrosshair.SetWeaponData(weaponData);
            _currentCrosshair = cachedCrosshair;
            return cachedCrosshair;
        }
        return CreateCrosshair(weaponData);
    }
   
    private ICrosshair CreateCrosshair(WeaponData weaponData)
    {
        if(!weaponData.WeaponCrosshairPrefab) return null;
        DisableCurrentCrosshair();
        var crosshair = Instantiate(weaponData.WeaponCrosshairPrefab, _crosshairCanvas.transform).GetComponent<Crosshair>();
        _crosshairCache[weaponData.weaponCrosshairRef] = crosshair;
        _currentCrosshair = crosshair;
        crosshair.SetWeaponData(weaponData);
        return crosshair;
    }

    public void DisableCurrentCrosshair()
    {
        if (_currentCrosshair == null) return;
        _currentCrosshair.gameObject.SetActive(false);
        _currentCrosshair = null;
    }

    public void ReleaseAllCrosshairAssets()
    {
        foreach (var kvp in _crosshairCache)
        {
            if(kvp.Value.gameObject)
                Addressables.ReleaseInstance(kvp.Value.gameObject);
        }

        _crosshairCache.Clear();
        _currentCrosshair = null;
    }

    private void OnDisable()
    {
        ReleaseAllCrosshairAssets();
    }
}