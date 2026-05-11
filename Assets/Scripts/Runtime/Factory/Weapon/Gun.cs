using PrimeTween;
using UnityEngine;

public class Gun : Weapon
{
    [SerializeField] private Transform _tip;
    [SerializeField] private Transform _weaponModel;

    private GunData GunData => WeaponData as GunData;
    private uint _currentAmmo;
    private uint _maxAmmo;

    public override void SetWeaponData(WeaponData data)
    {
        base.SetWeaponData(data);
        _maxAmmo = GunData.AmmoPerMagazine;
        _currentAmmo = _maxAmmo;
    }

    public override void Update() => base.Update();

    protected override void Attack()
    {
        if (_currentAmmo <= 0) { Reload(); return; }

        // barrel direction: -transform.right due to rotationOffset Y=+90
        Vector3 barrelDir = -transform.right;
        Vector3 targetPos = _tip.position + barrelDir * 3;

        Projectile projectile = FlyweightFactory.Spawn(GunData.projectileSettings) as Projectile;
        projectile.FlyweightInit(_tip.position, Quaternion.LookRotation(barrelDir));
        projectile.ShootProjectile(_tip.position, targetPos, GunData);
        _currentAmmo--;

        base.Attack();

        Tween.ShakeLocalPosition(_weaponModel,
            new Vector3(GunData.ShakeMagnitude, GunData.ShakeMagnitude, 0f),
            GunData.ShakeDuration,
            GunData.ShakeFrequency);
    }

    public void Reload()
    {
        if (_currentAmmo == _maxAmmo) return;
        _currentAmmo = _maxAmmo;
        Debug.Log("Gun Reloaded");
    }
}