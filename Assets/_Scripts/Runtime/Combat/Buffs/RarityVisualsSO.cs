using LokiInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "RarityVisuals_", menuName = "Scriptable Objects/Buffs/Rarity Visuals")]
public class RarityVisualsSO : ScriptableObject
{
    [TabGroup("Identity")]
    [SerializeField] private BuffRarity _rarity;

    [TabGroup("Identity")]
    [SerializeField, Range(0.01f, 100f), LabelText("Base Weight")]
    private float _baseWeight = 1f;

    [TabGroup("Visuals")]
    [SerializeField] private Color _accentColor = Color.white;

    [TabGroup("Visuals")]
    [SerializeField] private Sprite _frameSprite;

    [TabGroup("Visuals")]
    [SerializeField] private Sprite _backgroundSprite;

    [TabGroup("Visuals")]
    [SerializeField] private GameObject _cardFxPrefab;

    public BuffRarity Rarity => _rarity;
    public float BaseWeight => _baseWeight;
    public Color AccentColor => _accentColor;
    public Sprite FrameSprite => _frameSprite;
    public Sprite BackgroundSprite => _backgroundSprite;
    public GameObject CardFxPrefab => _cardFxPrefab;
}

public enum BuffRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary,
}

