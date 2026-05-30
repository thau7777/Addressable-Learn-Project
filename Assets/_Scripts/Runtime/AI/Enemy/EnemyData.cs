using Cysharp.Threading.Tasks;
using LokiInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Flyweight Settings/EnemyData")]
public class EnemyData : FlyweightSettings
{
    [TabGroup("Stats")]
    public float maxHP = 50f;
    [TabGroup("Stats")]
    public float baseDamage = 10f;
    [TabGroup("Stats")]
    public float moveSpeed = 3f;
    [TabGroup("Stats")]
    public float expDrop = 5f;
    [TabGroup("Stats")]
    public float ripenessRate = 1f;
    [TabGroup("Stats")]
    public float spawnDuration = 0.5f;

    [TabGroup("Strategy")]
    [Required] public MovementStrategyData movementStrategy;
    [TabGroup("Strategy")]
    [Required] public AttackStrategyData attackStrategy;

    [TabGroup("On Death")]
    public FlyweightSettings explodeVfxSettings;
    [TabGroup("On Death")]
    public bool splitsOnDeath;
    [TabGroup("On Death")]
    [ShowIf("splitsOnDeath")]
    public EnemyData splitEnemyData;
    [TabGroup("On Death")]
    [ShowIf("splitsOnDeath")]
    public int splitCount = 3;


    public override async UniTask<bool> LoadPrefabAsync()
    {
        if (attackStrategy.attackFlyweightSettings != null) await attackStrategy.attackFlyweightSettings.LoadPrefabAsync();
        if (explodeVfxSettings != null) await explodeVfxSettings.LoadPrefabAsync();
        if (splitsOnDeath && splitEnemyData != null) await splitEnemyData.LoadPrefabAsync();
        return await base.LoadPrefabAsync();
    }

    public override Flyweight Create()
    {
        GameObject enemyGo = Instantiate(Prefab);
        enemyGo.name = Prefab.name;
        EnemyController enemy = enemyGo.GetOrAdd<EnemyController>();
        enemy.EnemyInit(this, movementStrategy.CreateStrategy(), attackStrategy.CreateStrategy());
        return enemy;
    }

    public override void OnGet(Flyweight f)
    {
        base.OnGet(f);
        EnemyController enemy = f as EnemyController;
        enemy.ResetEnemy();
    }
}
