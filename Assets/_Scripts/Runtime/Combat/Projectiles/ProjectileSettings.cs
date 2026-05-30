using Cysharp.Threading.Tasks;
using LokiInspector;
using UnityEngine;

public abstract class ProjectileSettings : FlyweightSettings
{

    public LayerMask collisionLayers;
    [TabGroup("Damage")]
    public bool dealsDamage = true;

    [TabGroup("On Hit")]
    public OneShotVfxSettings onImpactVfx;

    public override async UniTask<bool> LoadPrefabAsync()
    {
        if (onImpactVfx != null)
            await onImpactVfx.LoadPrefabAsync();
        return await base.LoadPrefabAsync();
    }
}
