using UnityEngine;
using UnityEngine.InputSystem;
using System;
public class GameplayActions : MyInputActions.IGameplayActions
{
    public event Action<Vector2> onMove;
    public event Action<Vector2> onMouseMove;
    public event Action<bool> onShoot;
    public event Action onReload;
    public event Action<uint> onEquipWeapon; 

    public void OnMouseMove(InputAction.CallbackContext context)
    {
        if (context.performed)
            onMouseMove?.Invoke(context.ReadValue<Vector2>());
    }
    public void OnEquipWeapon_1(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            onEquipWeapon?.Invoke(1);
        }
    }

    public void OnEquipWeapon_2(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            onEquipWeapon?.Invoke(2);
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        onMove?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            onReload?.Invoke();
        }
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            onShoot?.Invoke(true);
        }
        else if (context.canceled)
        {
            onShoot?.Invoke(false);
        }
    }
}
