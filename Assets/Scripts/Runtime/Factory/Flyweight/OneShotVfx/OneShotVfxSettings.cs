using LokiInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "OneShotVfxSettings", menuName = "Scriptable Objects/Flyweight Settings/OneShotVfxSettings")]
public class OneShotVfxSettings : FlyweightSettings
{
    [TabGroup("Damage")]
    public bool dealsDamage;

    [TabGroup("Damage")]
    [ShowIf("dealsDamage")]
    public float baseDamage = 10;

    [TabGroup("Damage")]
    [ShowIf("dealsDamage")]
    public LayerMask hitboxLayers;

    [TabGroup("Damage")]
    [ShowIf("dealsDamage")]
    [LabelText("Activate Delay")]
    public float hitboxActivateDelay;

    [TabGroup("Damage")]
    [ShowIf("dealsDamage")]
    [LabelText("Active Duration")]
    public float hitboxActiveDuration;

    public override Flyweight Create()
    {
        GameObject obj = Instantiate(Prefab);
        obj.name = Prefab.name;
        var flyweight = obj.GetOrAdd<OneShotVfx>();
        flyweight.settings = this;
        return flyweight;
    }

    public override void OnGet(Flyweight f)
    {
        f.transform.SetParent(null);
        // Do NOT SetActive here — caller must FlyweightInit(pos, rot) then SetActive(true)
        // so the effect plays at the correct world position
    }
}
