using Cysharp.Threading.Tasks;
using LokiInspector;
using PrimeTween;
using UnityEngine;

[CreateAssetMenu(fileName = "GunData", menuName = "Scriptable Objects/Weapon/GunData", order = 0)]
public class RangedWeaponData : WeaponData
{
    [TabGroup("Basic Settings")]
    [Required] public ProjectileSettings projectileSettings;
    [TabGroup("Basic Settings")]
    [Required] public OneShotVfxSettings muzzleVfxSettings;

    [SerializeField, TabGroup("Basic Settings")]
    private uint _ammoPerMagazine = 30;
    [SerializeField, TabGroup("Basic Settings")]
    private uint _magazineCapacity = 3;
    [SerializeField, TabGroup("Basic Settings")]
    private float _reloadTime = 1.5f;
    [SerializeField, TabGroup("Basic Settings")]
    private float _bulletSpeed = 50f;


    [SerializeField, TabGroup("Animation Settings")]
    private float _recoilDistance = 0.15f;
    [SerializeField, TabGroup("Animation Settings")]
    private float _recoilDuration = 0.05f;
    [SerializeField, TabGroup("Animation Settings")]
    private float _recoilReturnDuration = 0.12f;
    [SerializeField, TabGroup("Animation Settings")]
    private Ease _recoilEase = Ease.OutQuad;
    [SerializeField, TabGroup("Animation Settings")]
    private Ease _recoilReturnEase = Ease.InOutSine;

    public uint AmmoPerMagazine => _ammoPerMagazine;
    public uint MagazineCapacity => _magazineCapacity;
    public float ReloadTime => _reloadTime;
    public float BulletSpeed => _bulletSpeed;
    public float RecoilDistance => _recoilDistance;
    public float RecoilDuration => _recoilDuration;
    public float RecoilReturnDuration => _recoilReturnDuration;
    public Ease RecoilEase => _recoilEase;
    public Ease RecoilReturnEase => _recoilReturnEase;

    public override async UniTask LoadWeaponAssetsAsync()
    {
        await base.LoadWeaponAssetsAsync();
        if (projectileSettings != null)
        {
            await projectileSettings.LoadPrefabAsync();
            await muzzleVfxSettings.LoadPrefabAsync();
        }
    }
}
