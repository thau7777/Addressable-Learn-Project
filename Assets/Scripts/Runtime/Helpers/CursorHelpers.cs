using UnityEngine;
using UnityEngine.SceneManagement;

public class CursorHelpers
{
    public static bool GetCursorWorldPositionOnFlatSurface(Vector2 screenPos,out Vector3 position)
    {
        Ray ray = CamHelpers.Cam.ScreenPointToRay(screenPos);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        if (groundPlane.Raycast(ray, out float dist))
        {
            position = ray.GetPoint(dist);
            return true;
        }
        position = Vector3.zero;
        return false;
    }
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
