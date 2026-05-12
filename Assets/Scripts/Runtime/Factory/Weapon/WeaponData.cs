using Cysharp.Threading.Tasks;
using LokiInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public abstract class WeaponData : ScriptableObject
{
    [TabGroup("References")]
    public AssetReference weaponPrefabRef;
    [TabGroup("References")]
    public AssetReference weaponIconRef;

    [TabGroup("Basic Info"), SerializeField]
    protected bool isAutomatic = true;
    [TabGroup("Basic Info"), SerializeField]
    protected float attackRate = 0.1f;
    [TabGroup("Basic Info"), SerializeField]
    protected float weaponRange = 5f;
    public float WeaponRange => weaponRange;

    public GameObject WeaponPrefab { get; set; }
    public GameObject WeaponIconPrefab { get; set; }

    private AsyncOperationHandle<GameObject> _weaponAsyncOperationHandle;
    private AsyncOperationHandle<GameObject> _iconAsyncOperationHandle;
    public bool IsAutomatic => isAutomatic;
    public float AttackRate => attackRate; // expose cho Weapon d�ng

    public virtual async UniTask LoadWeaponAssetsAsync()
    {
        if (WeaponPrefab == null && weaponPrefabRef.RuntimeKeyIsValid())
        {
            _weaponAsyncOperationHandle = Addressables.LoadAssetAsync<GameObject>(weaponPrefabRef);
            await _weaponAsyncOperationHandle.ToUniTask();
            if (_weaponAsyncOperationHandle.Status == AsyncOperationStatus.Succeeded)
                WeaponPrefab = _weaponAsyncOperationHandle.Result;
        }

        if (WeaponIconPrefab == null && weaponIconRef.RuntimeKeyIsValid())
        {
            _iconAsyncOperationHandle = Addressables.LoadAssetAsync<GameObject>(weaponIconRef);
            await _iconAsyncOperationHandle.ToUniTask();
            if (_iconAsyncOperationHandle.Status == AsyncOperationStatus.Succeeded)
                WeaponIconPrefab = _iconAsyncOperationHandle.Result;
        }
    }

    

    public virtual void UnloadWeaponAssets()
    {
        if (_weaponAsyncOperationHandle.IsValid())
        {
            Addressables.Release(_weaponAsyncOperationHandle);
            WeaponPrefab = null;
        }

        if (_iconAsyncOperationHandle.IsValid())
        {
            Addressables.Release(_iconAsyncOperationHandle);
            WeaponIconPrefab = null;
        }
    }

}
