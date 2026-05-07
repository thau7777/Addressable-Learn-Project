using System;
using Unity.Cinemachine;
using UnityEngine;

public interface IWeapon
{
    public WeaponData WeaponData { get; set; }
    public bool IsInitialized { get; set; }
    void OnEquip(Transform user);
    void OnUnequip();
    void SetWeaponData(WeaponData data);
    void OnAttackTrigger(bool isAttacking);
    void RegisterActionOnAttack(Action listener);
    void DeregisterAllListener();
    bool IsEquals(WeaponData data);
}
public abstract class Weapon : MonoBehaviour, IWeapon
{
    public WeaponData WeaponData { get; set; }
    public bool IsInitialized { get; set; }

    public Vector3 positionOnEquip;
    public Vector3 rotationOnEquip;

    protected bool _isAttacking;

    private event Action OnAttack;
    public void RegisterActionOnAttack(Action listener)
    {
        OnAttack += listener;
    }

    public void DeregisterAllListener()
    {
        if (OnAttack == null) return;
        foreach (Delegate d in OnAttack.GetInvocationList())
        {
            OnAttack -= (Action)d;
        }
    }
    public virtual void SetWeaponData(WeaponData data)
    {
        WeaponData = data;
    }

    public virtual void Update()
    {
        if (_isAttacking && WeaponData.CanAttack)
        {
            Attack();
        }
    }
    public void OnAttackTrigger(bool isAttacking)
    {
        _isAttacking = isAttacking;
    }
    protected virtual void Attack()
    {
        WeaponData.ResetAttackElapsedTime();
        OnAttack?.Invoke();
    }
    public void OnEquip(Transform user)
    {
        if(IsInitialized) return;
        IsInitialized = true;
        transform.SetParent(user);
        transform.localPosition = positionOnEquip;
        transform.localRotation = Quaternion.Euler(rotationOnEquip);

    }

    public void OnUnequip()
    {
        _isAttacking = false;
        gameObject.SetActive(false);
    }
    public bool IsEquals(WeaponData data)
    {
        return WeaponData == data;
    }

}
