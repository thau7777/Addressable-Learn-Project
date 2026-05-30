using UnityEngine;

public interface IWeapon
{
    WeaponData WeaponData { get; }
    Transform Transform { get; }
    void OnEquip(Transform user, Vector3 localPosition);
    void OnUnequip();
    bool IsEquippedWith(WeaponData data);
}

public abstract class Weapon : MonoBehaviour, IWeapon
{
    public WeaponData WeaponData { get; private set; }
    public Transform Transform => transform;

    protected Transform _currentTarget;

    protected abstract float Damage { get; }
    protected abstract float AttackRate { get; }
    protected abstract float RangeMul { get; }

    protected float Range => WeaponData.BaseWeaponRange * RangeMul;

    public float CurrentDamage => Damage;
    public float CurrentAttackRate => AttackRate;
    public float CurrentRange => Range;

    private bool CanAttack => _attackRateElapsedTime >= 1f / AttackRate;

    private float _attackRateElapsedTime;
    private Transform _user;
    private int _enemyLayerMask;
    private readonly Collider[] _overlapResults = new Collider[20];

    #region Initialization
    public virtual void SetWeaponData(WeaponData data)
    {
        WeaponData = data;
    }

    public void OnEquip(Transform user, Vector3 localPosition)
    {
        _user = user;
        _enemyLayerMask = LayerMask.GetMask("Enemy");
        _attackRateElapsedTime = 1f / AttackRate;
        transform.SetParent(user);
        transform.localPosition = localPosition;
    }

    public virtual void OnUnequip()
    {
        _currentTarget = null;
    }
    #endregion

    #region Loop
    private void Update()
    {
        _attackRateElapsedTime += Time.deltaTime;
        _currentTarget = GetClosestEnemy();

        if (_currentTarget != null)
        {
            FaceTarget(_currentTarget);
            if (CanAttack)
                Attack();
        }

        OnUpdate();
    }

    protected virtual void OnUpdate() { }

    private Transform GetClosestEnemy()
    {
        int count = Physics.OverlapSphereNonAlloc(_user.position, Range, _overlapResults, _enemyLayerMask);

        Transform closest = null;
        float closestDist = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            float dist = Vector3.SqrMagnitude(_user.position - _overlapResults[i].transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = _overlapResults[i].transform;
            }
        }

        return closest;
    }

    private void FaceTarget(Transform target)
    {
        Vector3 dir = target.position - transform.position;
        dir.y = 0f;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);
    }

    #endregion

    #region Outside Calls
    protected virtual void Attack()
    {
        _attackRateElapsedTime = 0f;
    }

    public bool IsEquippedWith(WeaponData data) => WeaponData == data;
    #endregion
}
