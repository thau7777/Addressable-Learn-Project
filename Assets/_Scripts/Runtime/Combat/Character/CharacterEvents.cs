using UnityEngine;

public struct PlayerSpawnedEvent : IEvent
{
    public Transform PlayerTransform { get; }
    public CharacterInfos CharacterInfos { get; }

    public PlayerSpawnedEvent(Transform playerTransform, CharacterInfos characterInfos)
    {
        PlayerTransform = playerTransform;
        CharacterInfos = characterInfos;
    }
}
