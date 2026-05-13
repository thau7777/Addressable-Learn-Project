using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : Singleton<WeaponManager>
{
    [SerializeField] private float _weaponOrbitRadius = 1.5f;
    [SerializeField] private int _maxWeaponSlots = 6;

    private const float StartAngleDeg = 180f;
    private readonly List<IWeapon> _activeWeapons = new();
    private Transform _playerTransform;

    public void Initialize(Transform playerTransform) => _playerTransform = playerTransform;

    public void EquipWeapon(WeaponData weaponData)
    {
        if (_activeWeapons.Count >= _maxWeaponSlots || _playerTransform == null) return;

        IWeapon weapon = WeaponFactory.Instance.GetWeapon(weaponData);
        if (weapon == null) return;

        _activeWeapons.Add(weapon);
        weapon.OnEquip(_playerTransform, CalculateSlotPosition(_activeWeapons.Count - 1, _activeWeapons.Count));
        RefreshWeaponPositions();
    }

    public void UnequipWeapon(WeaponData weaponData)
    {
        IWeapon weapon = _activeWeapons.Find(w => w.IsEquippedWith(weaponData));
        if (weapon == null) return;

        weapon.OnUnequip();
        _activeWeapons.Remove(weapon);
        Destroy(weapon.Transform.gameObject);
        RefreshWeaponPositions();
    }

    private void RefreshWeaponPositions()
    {
        int count = _activeWeapons.Count;
        for (int i = 0; i < count; i++)
            _activeWeapons[i].Transform.localPosition = CalculateSlotPosition(i, count);
    }

    private Vector3 CalculateSlotPosition(int index, int total)
    {
        float rad = (StartAngleDeg + 360f / total * index) * Mathf.Deg2Rad;
        return new Vector3(
            _weaponOrbitRadius * Mathf.Cos(rad),
            0f,
            _weaponOrbitRadius * Mathf.Sin(rad)
        );
    }
}
