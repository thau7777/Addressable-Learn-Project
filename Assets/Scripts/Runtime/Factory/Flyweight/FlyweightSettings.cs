using LokiInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
public abstract class FlyweightSettings : ScriptableObject
{
    [TabGroup("Pool Settings")]
    public bool collectionCheck = true;

    [TabGroup("Pool Settings")]
    public int defaultCapacity = 10;

    [TabGroup("Pool Settings")]
    public int maxPoolSize = 100;

    [TabGroup("Flyweight Info")]
    public AssetReference prefabRef;
    public GameObject Prefab { get; set; }
    public abstract Flyweight Create();

    public virtual void OnGet(Flyweight f)
    {
        f.transform.SetParent(null);
        f.gameObject.SetActive(true);
    }
    public virtual void OnRelease(Flyweight f) => f.gameObject.SetActive(false);
    public virtual void OnDestroyPoolObject(Flyweight f) => Destroy(f.gameObject);
}
