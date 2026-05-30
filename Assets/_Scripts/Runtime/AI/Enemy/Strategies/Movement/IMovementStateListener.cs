public interface IMovementStateListener
{
    void OnMoveEnter(IEnemyContext owner);
    void OnMoveExit(IEnemyContext owner);
}
