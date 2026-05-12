using UnityEngine;
using UnityEngine.SceneManagement;

public class CursorHelpers
{
    
    public static void Hide(bool confine = false)
    {
        Cursor.visible = false;
        Cursor.lockState = confine ? CursorLockMode.Confined : CursorLockMode.None;
    }

    public static void Show(bool confine = false)
    {
        Cursor.visible = true;
        Cursor.lockState = confine ? CursorLockMode.Confined : CursorLockMode.None;
    }

    public static void Toggle(bool confine = false)
    {
        if (Cursor.visible) Hide(confine);
        else Show(confine);
    }
}
