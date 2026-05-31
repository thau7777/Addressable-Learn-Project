using System;
using Cysharp.Threading.Tasks;
using LokiInspector;
using UnityEngine;

// What an attack spawns on each strike. Composed into StagedAttackData via [SerializeReference].
// New strike behavior = new IStrikeSpawner. The strategy never changes.

public interface IStrikeSpawner
{
    void Spawn(IEnemyContext owner, Transform target);
    UniTask PreloadAsync();
}

/// <summary>Spawns a pooled OneShotVfx (damaging hitbox) at the owner or on the target's ground spot.</summary>
[Serializable]
public class VfxStrikeSpawner : IStrikeSpawner
{
    public enum SpawnAt { OwnerPosition, TargetGroundPosition }

    [Required] public FlyweightSettings vfxSettings;
    public SpawnAt spawnAt = SpawnAt.OwnerPosition;
    [Tooltip("Local offset (relative to owner facing) applied to the spawn position.")]
    public Vector3 localOffset;

    public void Spawn(IEnemyContext owner, Transform target)
    {
        if (vfxSettings == null || owner == null) return;
        var fw = FlyweightFactory.Spawn(vfxSettings);
        if (fw == null) return;

        Vector3 basePos = spawnAt == SpawnAt.TargetGroundPosition && target != null
            ? new Vector3(target.position.x, owner.transform.position.y, target.position.z)
            : owner.transform.position;
        Vector3 pos = basePos + owner.transform.TransformDirection(localOffset);

        fw.FlyweightInit(pos, owner.transform.rotation);
        if (fw is OneShotVfx vfx) vfx.OneShotVfxInit(owner.Damage);
    }

    public async UniTask PreloadAsync()
    {
        if (vfxSettings != null) await vfxSettings.LoadPrefabAsync();
    }
}

/// <summary>Fires a pooled projectile toward the live target, routing the owner's runtime damage.</summary>
[Serializable]
public class ProjectileStrikeSpawner : IStrikeSpawner
{
    [Required] public ProjectileSettings projectileSettings;
    public float bulletSpeed = 15f;
    public float bulletRange = 20f;
    [Tooltip("Local offset (relative to owner facing) for the projectile spawn point.")]
    public Vector3 spawnLocalOffset;

    public void Spawn(IEnemyContext owner, Transform target)
    {
        if (projectileSettings == null || owner == null || target == null) return;
        var fw = FlyweightFactory.Spawn(projectileSettings);
        if (fw is not Projectile projectile) return;

        Vector3 spawnPos = owner.transform.position + owner.transform.TransformDirection(spawnLocalOffset);
        projectile.FlyweightInit(spawnPos, owner.transform.rotation);
        projectile.ShootProjectile(
            spawnPos,
            target.position,
            new RuntimeProjectileLaunchData(bulletSpeed, owner.Damage, bulletRange));
    }

    public async UniTask PreloadAsync()
    {
        if (projectileSettings != null) await projectileSettings.LoadPrefabAsync();
    }

    private readonly struct RuntimeProjectileLaunchData : IProjectileLaunchData
    {
        public RuntimeProjectileLaunchData(float speed, float damage, float range)
        {
            Speed = speed;
            Damage = damage;
            Range = range;
        }
        public float Speed { get; }
        public float Damage { get; }
        public float Range { get; }
    }
}
