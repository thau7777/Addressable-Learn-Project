using UnityEngine;
using UnityEngine.InputSystem;
public class Gun : Weapon
{
    [SerializeField]
    private Transform _tip;

    [SerializeField, Range(0,5)]
    private float _offsetToTargetPos = 3f;

    [SerializeField]
    private Quaternion _rotationOffset = Quaternion.Euler(1f, 90f, 1f);
    private GunData GunData => WeaponData as GunData;
    private uint currentAmmo;
    private uint maxAmmo;                      // <--


    public override void SetWeaponData(WeaponData data)
    {
        base.SetWeaponData(data);
        maxAmmo = GunData.AmmoPerMagazine;
        currentAmmo = maxAmmo;
    }
    public override void Update()
    {
        base.Update();
        RotateHandler();
    }   
    private void RotateHandler()
    {
        if (!CursorHelpers.GetCursorWorldPositionOnFlatSurface(Mouse.current.position.ReadValue(),out Vector3 worldPos)) return;

        Vector3 lookDir = worldPos - transform.position;
        lookDir.y = 0f;

        if (lookDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDir) * _rotationOffset;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 100f);
        }
    }
    protected override void Attack()
    {
        if (currentAmmo <= 0)
        {
            Reload();
            return;
        }

        Vector3 targetPos = transform.forward;
        if (CursorHelpers.GetCursorWorldPositionOnFlatSurface(Mouse.current.position.ReadValue(), out Vector3 mouseWorldPos))
        {
            targetPos = mouseWorldPos;

            Vector3 camFlatForward = CamHelpers.GetCamFlatForward();
            targetPos -= camFlatForward * (_tip.position.y / _offsetToTargetPos);
        }
        targetPos.y = _tip.position.y;

        Projectile projectile = FlyweightFactory.Spawn(GunData.projectileSettings) as Projectile;
        projectile.FlyweightInit(_tip.position, Quaternion.LookRotation(targetPos - _tip.position));
        projectile.ShootProjectile(_tip.position, targetPos, GunData);
        currentAmmo--;
        base.Attack();
    }

    public void Reload()
    {
        if (currentAmmo == maxAmmo) return;
        currentAmmo = maxAmmo;
        Debug.Log("Gun Reloaded");
    }
}