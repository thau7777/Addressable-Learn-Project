using UnityEngine;

public interface IEvent { }

public struct EnemyDeathEvent : IEvent
{
    public Transform EnemyTransform { get; }
    public EnemyDeathEvent(Transform enemyTransform)
    {
        EnemyTransform = enemyTransform;
    }
}

public struct ExpPickupEvent : IEvent
{
    public float ExpAmount { get; }
    public ExpPickupEvent(float expAmount)
    {
        ExpAmount = expAmount;
    }
}

public struct ExpProgressChangedEvent : IEvent
{
    public float value01 { get; }
    public ExpProgressChangedEvent(float value01)
    {
        this.value01 = value01;
    }
}

public struct LevelUpEvent : IEvent
{
    public int NewLevel { get; }
    public LevelUpEvent(int newLevel)
    {
        NewLevel = newLevel;
    }
}
