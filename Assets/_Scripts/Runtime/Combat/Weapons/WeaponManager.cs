using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : Singleton<WeaponManager>
{
    [SerializeField] private float _weaponOrbitRadius = 1.5f;
    [SerializeField] private int _maxWeaponSlots = 6;

    private const float StartAngleDeg = 180f;
    private readonly List<IWeapon> _activeWeapons = new();
    private Transform _playerTransform;

    private EventBinding<AddWeaponEvent> _addWeaponBinding;
    private EventBinding<RemoveWeaponEvent> _removeWeaponBinding;
    private EventBinding<PlayerSpawnedEvent> _playerSpawnedBinding;

    private void OnEnable()
    {
        _playerSpawnedBinding = new EventBinding<PlayerSpawnedEvent>(OnPlayerSpawned);
        EventBus<PlayerSpawnedEvent>.Register(_playerSpawnedBinding);

        _addWeaponBinding = new EventBinding<AddWeaponEvent>(EquipWeapon);
        EventBus<AddWeaponEvent>.Register(_addWeaponBinding);

        _removeWeaponBinding = new EventBinding<RemoveWeaponEvent>(OnRemoveWeapon);
        EventBus<RemoveWeaponEvent>.Register(_removeWeaponBinding);
    }
    private void OnDisable()
    {
        EventBus<PlayerSpawnedEvent>.Deregister(_playerSpawnedBinding);
        EventBus<AddWeaponEvent>.Deregister(_addWeaponBinding);
        EventBus<RemoveWeaponEvent>.Deregister(_removeWeaponBinding);
    }

    private void OnPlayerSpawned(PlayerSpawnedEvent evt) => _playerTransform = evt.PlayerTransform;
    private void OnRemoveWeapon(RemoveWeaponEvent evtData) => UnequipWeapon(evtData.WeaponData);

    #region Weapon Equip/Unequip
    private bool TryCreateWeapon(WeaponData data, out IWeapon weapon)
    {
        if (!data.WeaponPrefab)
        {
            weapon = null;
            return false;
        }
        Weapon newWeapon = Instantiate(data.WeaponPrefab).GetComponent<Weapon>();

        newWeapon.SetWeaponData(data);
        weapon = newWeapon;
        return true;
    }
    public void EquipWeapon(AddWeaponEvent evtData)
    {
        if (_activeWeapons.Count >= _maxWeaponSlots || _playerTransform == null || !TryCreateWeapon(evtData.WeaponData, out IWeapon weapon)) return;

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

    public T FirstWeaponOfType<T>() where T : Weapon
    {
        for (int i = 0; i < _activeWeapons.Count; i++)
            if (_activeWeapons[i] is T typed) return typed;
        return null;
    }
    #endregion
}
