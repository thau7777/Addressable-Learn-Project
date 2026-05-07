using Cysharp.Threading.Tasks;
using LokiInspector;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;

public abstract class WeaponData : ScriptableObject
{
    [TabGroup("References")]
    public AssetReference weaponPrefabRef;
    [TabGroup("References")]
    public AssetReference weaponIconRef;
    [TabGroup("References")]
    public AssetReference weaponCrosshairRef;

    public GameObject WeaponPrefab { get; set; }
    public GameObject WeaponIconPrefab { get; set; }
    public GameObject WeaponCrosshairPrefab { get; set; }

    [TabGroup("Basic Info"), SerializeField]
    protected bool isAutomatic = true;
    [TabGroup("Basic Info"),SerializeField]
    protected float attackRate = 0.1f;
    protected float attackRateElapsedTime = 0f;


    public bool IsAutomatic => isAutomatic;
    public bool CanAttack => attackRateElapsedTime >= attackRate;

    private CancellationTokenSource _attackRateCts;

    public virtual void OnWeaponChose()
    {
        if (weaponPrefabRef.RuntimeKeyIsValid())
        {

            Addressables.LoadAssetAsync<GameObject>(weaponPrefabRef).Completed += handle =>
            {
                if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                {
                    WeaponPrefab = handle.Result;
                }
            };
        }
        if (weaponIconRef.RuntimeKeyIsValid())
        {
            Addressables.LoadAssetAsync<GameObject>(weaponIconRef).Completed += handle =>
            {
                if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                {
                    WeaponIconPrefab = handle.Result;
                }
            };
        }
        if (weaponCrosshairRef.RuntimeKeyIsValid())
        {
            Addressables.LoadAssetAsync<GameObject>(weaponCrosshairRef).Completed += handle =>
            {
                if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                {
                    WeaponCrosshairPrefab = handle.Result;
                }
            };
        }
        else
        {
            weaponCrosshairRef = CrosshairFactory.Instance.defaultCrosshairPrefabRef;
            if (weaponCrosshairRef.RuntimeKeyIsValid())
            {
                Addressables.LoadAssetAsync<GameObject>(weaponCrosshairRef).Completed += handle =>
                {
                    if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                    {
                        WeaponCrosshairPrefab = handle.Result;
                    }
                };
            }
        }
    }
    public void OnWeaponFirstCreate()
    {
        attackRateElapsedTime = attackRate;
        _attackRateCts = new CancellationTokenSource();
        StartFireRateCounter(_attackRateCts.Token).Forget();
    }

    private async UniTaskVoid StartFireRateCounter(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            attackRateElapsedTime += Time.deltaTime;
            await UniTask.Yield(ct);
        }
    }
    public void ResetAttackElapsedTime()
    {
        attackRateElapsedTime = 0f;
    }
    public void StopTask()
    {
        _attackRateCts?.Cancel();
        _attackRateCts?.Dispose();
        _attackRateCts = null;
    }
}
