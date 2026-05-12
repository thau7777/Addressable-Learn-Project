using UnityEngine;
using UnityEngine.UIElements;

// EnemyData.cs
[CreateAssetMenu(menuName = "Scriptable Objects/Flyweight Settings/EnemyData")]
public class EnemyData : FlyweightSettings
{
    [Header("Base Stats")]
    public float maxHP = 50f;
    public float moveSpeed = 3f;
    public float expDrop = 5f;
    public float ripenessRate = 1f;
    public float spawnDuration = 0.5f;

    [Header("Strategy")]
    public MovementStrategyData movementStrategy;
    public AttackStrategyData attackStrategy;

    [Header("On Death")]
    public GameObject fracturePrefab;
    //public JuiceType juiceType;

    [Header("Grape Split")]
    public bool splitsOnDeath;
    public GameObject splitPrefab;
    public int splitCount = 3;

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
