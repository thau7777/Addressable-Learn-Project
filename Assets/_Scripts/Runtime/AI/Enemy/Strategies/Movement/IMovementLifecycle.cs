public interface IMovementLifecycle
{
    void OnOwnerCreated(IEnemyContext owner);
    void OnOwnerReset();
}
