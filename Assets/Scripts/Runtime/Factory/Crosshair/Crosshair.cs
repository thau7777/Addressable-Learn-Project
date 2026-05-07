using UnityEngine;

public interface ICrosshair
{
    void SetWeaponData(WeaponData weaponData);
    void OnExecute();
    void ResetSpread();
}
public abstract class Crosshair : MonoBehaviour, ICrosshair
{
    protected WeaponData _weaponData;
    //protected void OnEnable()
    //{
    //    ResetSpread();
    //}
    public abstract void OnExecute();
    public abstract void ResetSpread();

    public void SetWeaponData(WeaponData weaponData)
    {
        _weaponData = weaponData;
        ResetSpread();
    }

}
