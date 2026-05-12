using PrimeTween;
using UnityEngine;

public class Gun : Weapon
{
    [SerializeField] private Transform _tip;
    [SerializeField] private Transform _weaponModel;

    private GunData GunData => WeaponData as GunData;
    private uint _currentAmmo;
    private uint _maxAmmo;
    private Vector3 _originLocalPos;
    private Sequence _recoilSequence;

    private void Awake()
    {
        _originLocalPos = _weaponModel.localPosition;
    }

    public override void SetWeaponData(WeaponData data)
    {
        base.SetWeaponData(data);
        _maxAmmo = GunData.AmmoPerMagazine;
        _currentAmmo = _maxAmmo;
    }


    protected override void Attack()
    {
        if (_currentAmmo <= 0) { Reload(); return; }

        Vector3 targetPos = _tip.position + _tip.forward;

        Projectile projectile = FlyweightFactory.Spawn(GunData.projectileSettings) as Projectile;
        projectile.FlyweightInit(_tip.position, Quaternion.LookRotation(_tip.forward));
        projectile.ShootProjectile(_tip.position, targetPos, GunData);
        _currentAmmo--;

        base.Attack();
        PlayRecoil();
    }

    private void PlayRecoil()
    {
        _recoilSequence.Stop();
        _weaponModel.localPosition = _originLocalPos;

        float attackInterval = 1f / WeaponData.AttackRate;
        Vector3 recoilPos = _originLocalPos - Vector3.forward * GunData.RecoilDistance;

        _recoilSequence = Sequence.Create()
            .Chain(Tween.LocalPosition(_weaponModel, recoilPos, GunData.RecoilDuration * attackInterval, GunData.RecoilEase))
            .Chain(Tween.LocalPosition(_weaponModel, _originLocalPos, GunData.RecoilReturnDuration * attackInterval, GunData.RecoilReturnEase));
    }

    public override void OnUnequip()
    {
        base.OnUnequip();
        _recoilSequence.Stop();
        _weaponModel.localPosition = _originLocalPos;
    }

    public void Reload()
    {
        if (_currentAmmo == _maxAmmo) return;
        _currentAmmo = _maxAmmo;
        Debug.Log("Gun Reloaded");
    }
}
