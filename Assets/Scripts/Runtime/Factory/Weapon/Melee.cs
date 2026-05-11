using PrimeTween;
using UnityEngine;

public class Melee : Weapon
{
    [SerializeField] private Transform _weaponModel;

    private Vector3 _originLocalPos;
    private Quaternion _originLocalRot;
    private Sequence _currentAnim;

    private MeleeData MeleeData => WeaponData as MeleeData;

    private void Awake()
    {
        _originLocalPos = _weaponModel.localPosition;
        _originLocalRot = _weaponModel.localRotation;
    }

    protected override void Attack()
    {
        base.Attack();

        _currentAnim.Stop();
        _weaponModel.localPosition = _originLocalPos;
        _weaponModel.localRotation = _originLocalRot;

        if (MeleeData.AttackAnim != null)
            _currentAnim = MeleeData.AttackAnim.Play(_weaponModel, _currentTarget, _originLocalPos, _originLocalRot, 1f / WeaponData.AttackRate);
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
}