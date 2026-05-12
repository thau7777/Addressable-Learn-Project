using UnityEngine;
using UnityEngine.InputSystem;
using System;
public class GameplayActions : MyInputActions.IGameplayActions
{
    public event Action<Vector2> onMove;

    public void OnMove(InputAction.CallbackContext context)
    {
        onMove?.Invoke(context.ReadValue<Vector2>());
    }

}
