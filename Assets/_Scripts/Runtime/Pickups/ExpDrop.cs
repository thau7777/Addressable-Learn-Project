using UnityEngine;

public class ExpDrop : Pickable
{
    private ExpDropSettings ExpSettings => base.settings as ExpDropSettings;
    protected override void OnPicked()
    {
        base.OnPicked();
        EventBus<ExpPickupEvent>.Raise(new ExpPickupEvent(ExpSettings.expAmount));
    }
}
