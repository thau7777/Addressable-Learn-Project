using UnityEngine;

public interface IEvent { }

public struct TestEvent : IEvent { }

public struct PlayerExampleEvent : IEvent
{
    public int health;
    public int mana;
}

