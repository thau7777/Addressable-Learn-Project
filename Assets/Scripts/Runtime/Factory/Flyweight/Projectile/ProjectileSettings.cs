using LokiInspector;
using UnityEngine;

public abstract class ProjectileSettings : FlyweightSettings
{

    public LayerMask collisionLayers;
    [TabGroup("Damage")]
    public bool dealsDamage = true;

    [TabGroup("On Hit")]
    public OneShotVfxSettings onImpactVfx;
}
