using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyController : Flyweight, IDamageable, IEnemyContext
{
    public Transform VisualRoot { get; private set; }
    public Vector3 VrOgScale { get; private set; }
    public Quaternion VrOgRotation { get; private set; }
    public EnemyData Data { get => settings as EnemyData; set => settings = value; }
    public Rigidbody Rb { get; private set; }
    public StateMachine SM { get; private set; }
    public Transform PlayerTarget { get; private set; }
    public IMovementStrategy MovementStrategy { get; private set; }
    public IAttackStrategy AttackStrategy { get; private set; }

    public float CurrentHP { get; private set; }
    public float Damage { get; private set; }
    public bool IsDead { get; private set; }

    public event Action<float> OnDamaged;

    private void Awake()
    {
        VisualRoot = transform.GetChild(0);
        VrOgScale = VisualRoot.localScale;
        VrOgRotation = VisualRoot.localRotation;
        Rb = GetComponent<Rigidbody>();
        Rb.isKinematic = true;
        Rb.interpolation = RigidbodyInterpolation.Interpolate;
        Rb.useGravity = false;
        Rb.constraints = RigidbodyConstraints.FreezeRotationX
                       | RigidbodyConstraints.FreezeRotationZ
                       | RigidbodyConstraints.FreezePositionY;

        GetComponent<HitFlash>().SubscribeOnDamaged();
    }

    public void EnemyInit(EnemyData data, IMovementStrategy movementStrategy, IAttackStrategy attackStrategy)
    {
        Data = data;
        Damage = data.baseDamage;
        MovementStrategy = movementStrategy;
        AttackStrategy = attackStrategy;
        if (movementStrategy is IMovementLifecycle lc) lc.OnOwnerCreated(this);

        SM = new StateMachine();
        SM.RegisterState(new EnemySpawn(this));
        SM.RegisterState(new EnemyIdle(this));
        SM.RegisterState(new EnemyMove(this));
        SM.RegisterState(new EnemyAttack(this));
        SM.RegisterState(new EnemyDie(this));
        SM.Init<EnemySpawn>();
    }

    public void ResetEnemy()
    {
        CurrentHP = Data.maxHP;
        Damage = Data.baseDamage;
        IsDead = false;
        GetComponent<Collider>().enabled = true;
        if (MovementStrategy is IMovementLifecycle lc) lc.OnOwnerReset();
        SM.ForceTransition<EnemySpawn>();
    }

    private void Update()
    {
        AttackStrategy?.Tick(Time.deltaTime);
        SM.Tick();
    }

    private void FixedUpdate() => SM.FixedTick();

    public void RotateToPlayer()
    {
        if (IsDead || !PlayerTarget) return;
        Vector3 dir = PlayerTarget.position - transform.position;
        dir.y = 0;
        dir = dir.normalized;
        Quaternion targetRotation = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 100f);
    }

    public void SetTarget(Transform target) => PlayerTarget = target;

    public void ModifyDamage(float delta) => Damage = Mathf.Max(0f, Damage + delta);

    public void TakeDamage(float amount)
    {
        if (IsDead) return;
        CurrentHP = Mathf.Max(0f, CurrentHP - amount);
        OnDamaged?.Invoke(amount);
        if (CurrentHP <= 0f) TriggerDeath();
    }

    private void TriggerDeath()
    {
        if (IsDead) return;
        IsDead = true;
        SM.ForceTransition<EnemyDie>();
    }
}
