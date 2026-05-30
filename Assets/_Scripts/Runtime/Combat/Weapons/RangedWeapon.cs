using PrimeTween;
using UnityEngine;

public class RangedWeapon : Weapon, IProjectileLaunchData
{
    [SerializeField] private Transform _tip;
    [SerializeField] private Transform _weaponModel;

    private RangedWeaponData RangedWeaponData => WeaponData as RangedWeaponData;
    private Vector3 _originLocalPos;
    private Sequence _recoilSequence;

    protected override float Damage     => CharacterStats.Current.RangedDamage;
    protected override float AttackRate => CharacterStats.Current.RangedAttackRate;
    protected override float RangeMul   => CharacterStats.Current.RangedRangeMul;

    // IProjectileLaunchData — reads live character stats so projectiles always see current values.
    float IProjectileLaunchData.Speed  => RangedWeaponData.BulletSpeed;
    float IProjectileLaunchData.Damage => Damage;
    float IProjectileLaunchData.Range  => Range;

    private void Awake()
    {
        _originLocalPos = _weaponModel.localPosition;
    }

    protected override void Attack()
    {
        Vector3 targetPos = _tip.position + _tip.forward;
        Projectile projectile = FlyweightFactory.Spawn(RangedWeaponData.projectileSettings) as Projectile;
        projectile.FlyweightInit(_tip.position, Quaternion.LookRotation(_tip.forward));
        projectile.ShootProjectile(_tip.position, targetPos, this);
        SpawnMuzzleFlash();
        base.Attack();
        PlayRecoil();
    }
    private void SpawnMuzzleFlash()
    {
        OneShotVfx muzzleFlash = FlyweightFactory.Spawn(RangedWeaponData.muzzleVfxSettings) as OneShotVfx;
        muzzleFlash.FlyweightInit(_tip.position, Quaternion.LookRotation(_tip.forward),_tip);
        muzzleFlash.OneShotVfxInit();
    }
    private void PlayRecoil()
    {
        _recoilSequence.Stop();
        _weaponModel.localPosition = _originLocalPos;

        float attackInterval = 1f / AttackRate;
        Vector3 recoilPos = _originLocalPos - Vector3.forward * RangedWeaponData.RecoilDistance;

        _recoilSequence = Sequence.Create()
            .Chain(Tween.LocalPosition(_weaponModel, recoilPos, RangedWeaponData.RecoilDuration * attackInterval, RangedWeaponData.RecoilEase))
            .Chain(Tween.LocalPosition(_weaponModel, _originLocalPos, RangedWeaponData.RecoilReturnDuration * attackInterval, RangedWeaponData.RecoilReturnEase));
    }

    public override void OnUnequip()
    {
        base.OnUnequip();
        _recoilSequence.Stop();
        _weaponModel.localPosition = _originLocalPos;
    }
}
