using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class FlyweightFactory : Singleton<FlyweightFactory>
{
    readonly Dictionary<FlyweightSettings, IObjectPool<Flyweight>> pools = new();
    public static Flyweight Spawn(FlyweightSettings settings) => instance.GetPoolFor(settings)?.Get();
    public static void ReturnToPool(Flyweight f) => instance.GetPoolFor(f.settings)?.Release(f);

    IObjectPool<Flyweight> GetPoolFor(FlyweightSettings settings)
    {
        IObjectPool<Flyweight> pool;

        if (pools.TryGetValue(settings, out pool)) return pool;

        pool = new ObjectPool<Flyweight>(
            settings.Create,
            settings.OnGet,
            settings.OnRelease,
            settings.OnDestroyPoolObject,
            settings.collectionCheck,
            settings.defaultCapacity,
            settings.maxPoolSize
        );
        pools.Add(settings, pool);
        return pool;
    }
    
} 
