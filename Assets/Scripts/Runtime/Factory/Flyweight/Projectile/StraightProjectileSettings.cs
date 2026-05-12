using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileSettings", menuName = "Scriptable Objects/Flyweight Settings/Projectile Settings/StraightProjectileSettings")]
public class StraightProjectileSettings : ProjectileSettings
{
    public LayerMask collisionLayers;
    public override Flyweight Create()
    {
        GameObject obj = Instantiate(Prefab);
        obj.name = Prefab.name;
        StraightProjectile flyweight = obj.GetOrAdd<StraightProjectile>();
        flyweight.settings = this;
        return flyweight;
    }
}
