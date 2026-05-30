using System.Text;
using LokiInspector;
using UnityEngine;

public abstract class BuffSO : ScriptableObject
{
    [TabGroup("Display")]
    [SerializeField] protected string _displayName;

    [TabGroup("Display")]
    [SerializeField] protected Sprite _icon;

    [TabGroup("Display")]
    [SerializeField] protected RarityVisualsSO _rarityVisuals;

    public string DisplayName => _displayName;
    public Sprite Icon => _icon;
    public RarityVisualsSO RarityVisuals => _rarityVisuals;
    public BuffRarity Rarity => _rarityVisuals != null ? _rarityVisuals.Rarity : BuffRarity.Common;

    public virtual void Apply()
    {
        TimeManager.Instance.PopScale(TimeModifier.Pause);
    }
    public abstract void Remove();

    public abstract string GetRuntimeDescription();

    protected static void AppendStatLine(StringBuilder sb, string statName, float current, float percent)
    {
        if (Mathf.Approximately(percent, 0f)) return;
        sb.AppendLine();
        float newValue = current * (1f + percent * 0.01f);
        string newColor = newValue < current ? "#FF6B6B" : "#90EE90";
        sb.Append(
            $"{statName} : " +
            $"{current:F1}" +
            $" → " +
            $"<size=115%><color={newColor}>{newValue:F1}</color></size>"
        );
    }
}
