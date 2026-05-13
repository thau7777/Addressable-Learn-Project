using Cysharp.Threading.Tasks;
using LokiInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
public abstract class FlyweightSettings : ScriptableObject
{
    [TabGroup("Pool Settings")]
    public bool collectionCheck = true;

    [TabGroup("Pool Settings")]
    public int defaultCapacity = 10;

    [TabGroup("Pool Settings")]
    public int maxPoolSize = 100;

    [TabGroup("Flyweight Info")]
    [Required] public AssetReference prefabRef;
    public GameObject Prefab { get; set; }
    private AsyncOperationHandle<GameObject> _prefabHandle;
    public abstract Flyweight Create();

    public virtual void OnGet(Flyweight f)
    {
        f.transform.SetParent(null);
        f.gameObject.SetActive(true);
    }
    public virtual void OnRelease(Flyweight f) => f.gameObject.SetActive(false);
    public virtual void OnDestroyPoolObject(Flyweight f) => Destroy(f.gameObject);

    public virtual async UniTask<bool> LoadPrefabAsync()
    {
        if (prefabRef == null) return false;
        if (Prefab != null) return true;
        _prefabHandle = prefabRef.LoadAssetAsync<GameObject>();
        await _prefabHandle.ToUniTask();
        if (_prefabHandle.Status == AsyncOperationStatus.Succeeded)
        {
            Prefab = _prefabHandle.Result;
            return true;
        }
        Debug.LogError($"Failed to load prefab for {name}");
        return false;
    }

    public virtual void ReleasePrefab()
    {
        if (_prefabHandle.IsValid())
            Addressables.Release(_prefabHandle);
        Prefab = null;
    }
}
