using Cysharp.Threading.Tasks;
using LokiInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "GunData", menuName = "Scriptable Objects/Weapon/GunData", order = 0)]
public class GunData : WeaponData
{
    [TabGroup("Basic Settings")]
    public ProjectileSettings projectileSettings;

    [SerializeField, TabGroup("Basic Settings")]
    private uint _ammoPerMagazine = 30;
    [SerializeField, TabGroup("Basic Settings")]
    private uint _magazineCapacity = 3;
    [SerializeField, TabGroup("Basic Settings")]
    private float _reloadTime = 1.5f;
    [SerializeField, TabGroup("Basic Settings")]
    private float _bulletSpeed = 50f;
    [SerializeField, TabGroup("Basic Settings")]
    private float _bulletDamage = 10f;

    [SerializeField, TabGroup("Spread Settings")]
    private float spreadOnShoot = 10f;
    [SerializeField, TabGroup("Spread Settings")]
    private float returnDuration = 0.3f;
    [SerializeField, TabGroup("Spread Settings")]
    private float maxSpreadThreshold = 30f;
    [SerializeField, TabGroup("Spread Settings")]
    private float spreadDuration = 0.05f;
    [SerializeField, TabGroup("Spread Settings")]
    private AnimationCurve spreadCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField, TabGroup("Spread Settings")]
    private AnimationCurve returnCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [SerializeField, TabGroup("Animation Settings")]
    private float _shakeMagnitude = 0.05f;
    [SerializeField, TabGroup("Animation Settings")]
    private float _shakeDuration = 0.1f;
    [SerializeField, TabGroup("Animation Settings")]
    private int _shakeFrequency = 20;

    public uint AmmoPerMagazine => _ammoPerMagazine;
    public uint MagazineCapacity => _magazineCapacity;
    public float ReloadTime => _reloadTime;

    public float BulletSpeed => _bulletSpeed;
    public float BulletDamage => _bulletDamage;



    public float SpreadOnShoot => spreadOnShoot;
    public float ReturnDuration => returnDuration;
    public float MaxSpreadThreshold => maxSpreadThreshold;
    public float SpreadDuration => spreadDuration;
    public AnimationCurve SpreadCurve => spreadCurve;
    public AnimationCurve ReturnCurve => returnCurve;

    public float ShakeMagnitude => _shakeMagnitude;
    public float ShakeDuration => _shakeDuration;
    public int ShakeFrequency => _shakeFrequency;

    public override void LoadWeaponAssets()
    {
        base.LoadWeaponAssets();
        if(projectileSettings != null)
        {
            projectileSettings.LoadPrefabAsync().Forget();
        }
    }
}
