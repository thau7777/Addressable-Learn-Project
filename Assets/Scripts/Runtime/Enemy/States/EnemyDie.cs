using UnityEngine;

public class EnemyDie : State<EnemyController>
{
    public EnemyDie(EnemyController owner) : base(owner) { }

    public override void OnEnter()
    {
        Owner.Rb.linearVelocity = Vector3.zero;
        Owner.GetComponent<Collider>().enabled = false;

        SpawnFracture();
        SpawnJuicePuddle();
        DropEXP();

        if (Owner.Data.splitsOnDeath)
            SpawnSplits();

        // EventBus<EnemyDeadEvent>.Raise(new EnemyDeadEvent(Owner));
        Owner.ReturnToPool();
    }

    public override void Tick() { }
    public override IState GetTransition() => null;
    public override void OnExit() { }

    private void SpawnFracture()
    {
        if (Owner.Data.fractureSettings == null) return;
        var fracture = FlyweightFactory.Spawn(Owner.Data.fractureSettings);
        if (fracture == null) return;
        fracture.FlyweightInit(Owner.transform.position, Owner.transform.rotation);
    }

    private void SpawnJuicePuddle()
    {
        // TODO: JuicePuddlePool.Instance.Spawn(Owner.Data.juiceType, Owner.transform.position);
    }

    private void DropEXP()
    {
        // TODO: ExpOrbPool.Instance.Spawn(Owner.transform.position, Owner.Data.expDrop);
    }

    private void SpawnSplits()
    {
        if (Owner.Data.splitEnemyData == null) return;
        for (int i = 0; i < Owner.Data.splitCount; i++)
        {
            Vector3 offset = Random.insideUnitSphere * 0.5f;
            offset.y = 0f;
            var splitEnemy = FlyweightFactory.Spawn(Owner.Data.splitEnemyData) as EnemyController;
            if (splitEnemy == null) continue;
            splitEnemy.FlyweightInit(Owner.transform.position + offset, Quaternion.identity);
            splitEnemy.SetTarget(Owner.PlayerTarget);
        }
    }
}
