using LokiInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "MeleeData", menuName = "Scriptable Objects/Weapon/MeleeData")]
public class MeleeWeaponData : WeaponData
{
    [TabGroup("Animation"), SerializeField]
    private WeaponAttackAnimSO _attackAnim;
    public WeaponAttackAnimSO AttackAnim => _attackAnim;
}
