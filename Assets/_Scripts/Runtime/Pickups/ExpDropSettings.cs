using UnityEngine;

[CreateAssetMenu(fileName = "ExpSettings", menuName = "Scriptable Objects/Flyweight Settings/Pickable Settings/ExpSettings")]
public class ExpDropSettings : PickableSettings
{
    public float expAmount;
    public override Flyweight Create()
    {
        GameObject obj = Instantiate(Prefab);
        obj.name = Prefab.name;
        ExpDrop flyweight = obj.GetOrAdd<ExpDrop>();
        flyweight.settings = this;
        return flyweight;
    }
}
