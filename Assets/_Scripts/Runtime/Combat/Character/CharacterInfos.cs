using LokiInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "CharacterInfos", menuName = "Scriptable Objects/Character/CharacterInfos")]
public class CharacterInfos : ScriptableObject
{
    [TabGroup("Display"), SerializeField] private string _displayName;
    [TabGroup("Display"), TextArea(2, 5), SerializeField] private string _description;
    [TabGroup("Display"), SerializeField] private Sprite _icon;

    [TabGroup("Visual"), Required, SerializeField] private AssetReference _characterModelRef;

    [TabGroup("Loadout"), Required, SerializeField] private WeaponData _defaultWeapon;

    [TabGroup("Base - Movement"), SerializeField] private float _baseMoveSpeed = 5f;
    [TabGroup("Base - Movement"), SerializeField] private float _baseMaxHealth = 100f;
    [TabGroup("Base - Movement"), SerializeField] private float _basePickupRadius = 2f;

    [TabGroup("Base - Combat"), SerializeField] private float _baseMeleeDamage = 5f;
    [TabGroup("Base - Combat"), SerializeField] private float _baseRangedDamage = 5f;
    [TabGroup("Base - Combat"), SerializeField] private float _baseMeleeAttackRate = 1f;
    [TabGroup("Base - Combat"), SerializeField] private float _baseRangedAttackRate = 1f;
    [TabGroup("Base - Combat"), SerializeField] private float _baseMeleeRangeMul = 1f;
    [TabGroup("Base - Combat"), SerializeField] private float _baseRangedRangeMul = 1f;

    public string DisplayName => _displayName;
    public string Description => _description;
    public Sprite Icon => _icon;
    public AssetReference CharacterModelRef => _characterModelRef;
    public WeaponData DefaultWeapon => _defaultWeapon;

    public float BaseMoveSpeed => _baseMoveSpeed;
    public float BaseMaxHealth => _baseMaxHealth;
    public float BasePickupRadius => _basePickupRadius;
    public float BaseMeleeDamage => _baseMeleeDamage;
    public float BaseRangedDamage => _baseRangedDamage;
    public float BaseMeleeAttackRate => _baseMeleeAttackRate;
    public float BaseRangedAttackRate => _baseRangedAttackRate;
    public float BaseMeleeRangeMul => _baseMeleeRangeMul;
    public float BaseRangedRangeMul => _baseRangedRangeMul;
}
