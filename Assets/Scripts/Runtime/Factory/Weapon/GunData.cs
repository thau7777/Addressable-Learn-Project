using LokiInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static UnityEngine.Android.AndroidGame;

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
    private float _bulletRange = 100f;
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



    public uint AmmoPerMagazine => _ammoPerMagazine;
    public uint MagazineCapacity => _magazineCapacity;
    public float ReloadTime => _reloadTime;

    public float BulletSpeed => _bulletSpeed;
    public float BulletRange => _bulletRange;
    public float BulletDamage => _bulletDamage;



    public float SpreadOnShoot => spreadOnShoot;
    public float ReturnDuration => returnDuration;
    public float MaxSpreadThreshold => maxSpreadThreshold;
    public float SpreadDuration => spreadDuration;
    public AnimationCurve SpreadCurve => spreadCurve;
    public AnimationCurve ReturnCurve => returnCurve;

    public override void OnWeaponChose()
    {
        base.OnWeaponChose();
        if(projectileSettings != null)
        {
            projectileSettings.prefabRef.LoadAssetAsync<GameObject>().Completed += handle =>
            {
                if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                {
                    projectileSettings.Prefab = handle.Result;
                }
            };
        }
    }
}
