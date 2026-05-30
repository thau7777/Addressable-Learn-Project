using PrimeTween;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MeleeWeapon : Weapon
{
    [SerializeField] private Transform _weaponModel;

    private Vector3 _originLocalPos;
    private Quaternion _originLocalRot;
    private Sequence _currentAnim;

    private MeleeWeaponData MeleeWeaponData => WeaponData as MeleeWeaponData;

    protected override float Damage     => CharacterStats.Current.MeleeDamage;
    protected override float AttackRate => CharacterStats.Current.MeleeAttackRate;
    protected override float RangeMul   => CharacterStats.Current.MeleeRangeMul;

    private Rigidbody _rb;
    private Collider _collider;
    private void Awake()
    {
        _originLocalPos = _weaponModel.localPosition;
        _originLocalRot = _weaponModel.localRotation;

        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = true;
        _rb.useGravity = false;

        _collider = GetComponentInChildren<Collider>();
        if (_collider != null)
            _collider.enabled = false;
    }

    protected override void Attack()
    {
        base.Attack();

        _currentAnim.Stop();
        _weaponModel.localPosition = _originLocalPos;
        _weaponModel.localRotation = _originLocalRot;

        if (MeleeWeaponData.AttackAnim != null)
            _currentAnim = MeleeWeaponData.AttackAnim.Play(_weaponModel, _currentTarget, _originLocalPos, Range, _originLocalRot, 1f / AttackRate, OnSwing, OnSwingEnd);
    }

    public void StopAnimation()
    {
        _currentAnim.Stop();
        _weaponModel.localPosition = _originLocalPos;
        _weaponModel.localRotation = _originLocalRot;
    }

    public override void OnUnequip()
    {
        base.OnUnequip();
        StopAnimation();
    }

    private void OnSwing()
    {
               // Enable collider at the right moment of the swing animation to apply damage
        if (_collider != null)
            _collider.enabled = true;
    }
    private void OnSwingEnd()
    {
        // Disable collider after the swing animation to prevent unintended damage
        if (_collider != null)
            _collider.enabled = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            if(other.transform.root.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(Damage);
            }
        }
    }
}
