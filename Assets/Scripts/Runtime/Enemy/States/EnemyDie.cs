using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class EnemyDie : State<EnemyController>
{
    public EnemyDie(EnemyController owner) : base(owner)
    {
    }

    public override void OnEnter()
    {
        Owner.Rb.linearVelocity = Vector3.zero;
        Owner.GetComponent<Collider>().enabled = false;

        SpawnFracture();
        SpawnJuicePuddle();
        DropEXP();

        if (Owner.Data.splitsOnDeath)
            SpawnSplits();

        // Raise event cho các system khác (AudioManager, ScoreManager...)
        //EventBus.Raise(new EnemyDeadEvent(Owner));
        Object.Destroy(Owner.gameObject, 0.1f); // nhường frame cho fracture spawn
    }

    public override void Tick() { }
    public override IState GetTransition() => null;
    public override void OnExit() { }

    private void SpawnFracture()
    {
        if (Owner.Data.fracturePrefab == null) return;
        Object.Instantiate(Owner.Data.fracturePrefab, Owner.transform.position, Owner.transform.rotation);
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
        if (Owner.Data.splitPrefab == null) return;
        for (int i = 0; i < Owner.Data.splitCount; i++)
        {
            Vector3 offset = Random.insideUnitSphere * 0.5f;
            offset.y = 0f;
            Object.Instantiate(Owner.Data.splitPrefab, Owner.transform.position + offset, Quaternion.identity);
        }
    }
}
