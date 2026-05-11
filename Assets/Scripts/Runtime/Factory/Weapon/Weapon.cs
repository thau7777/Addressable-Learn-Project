using System;
using UnityEngine;

public interface IWeapon
{
    public WeaponData WeaponData { get; set; }
    void OnEquip(Transform user, Vector3 localPosition);
    void OnUnequip();
    bool IsEquals(WeaponData data);
}
public abstract class Weapon : MonoBehaviour, IWeapon
{
    public Quaternion rotationOffset; // set trong Inspector per weapon
    public WeaponData WeaponData { get; private set; }

    private float _attackRateElapsedTime;
    private bool CanAttack => _attackRateElapsedTime >= 1f / WeaponData.AttackRate;

    private Transform _user;
    protected Transform _currentTarget;
    private int _enemyLayerMask;
    private readonly Collider[] _overlapResults = new Collider[20]; // non-alloc, tránh GC

    WeaponData IWeapon.WeaponData { get => WeaponData; set => WeaponData = value; }
    private event Action OnAttack;

    public virtual void SetWeaponData(WeaponData data) => WeaponData = data;

    public void OnEquip(Transform user, Vector3 localPosition)
    {
        _user = user;
        _enemyLayerMask = LayerMask.GetMask("Enemy");
        _attackRateElapsedTime = 1f / WeaponData.AttackRate;
        transform.SetParent(user);
        transform.localPosition = localPosition;
    }

    public virtual void OnUnequip()
    {
        _currentTarget = null;
    }

    public virtual void Update()
    {
        _attackRateElapsedTime += Time.deltaTime;
        _currentTarget = GetClosestEnemy();

        if (_currentTarget != null && CanAttack)
        {
            FaceTarget(_currentTarget);
            Attack();
        }
    }

    private Transform GetClosestEnemy()
    {
        int count = Physics.OverlapSphereNonAlloc(_user.position, WeaponData.WeaponRange, _overlapResults, _enemyLayerMask);

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
            transform.rotation = Quaternion.LookRotation(dir) * rotationOffset;
    }

    protected virtual void Attack()
    {
        _attackRateElapsedTime = 0f;
        OnAttack?.Invoke();
    }

    public bool IsEquals(WeaponData data) => WeaponData == data;
}
