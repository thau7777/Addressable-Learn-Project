using UnityEngine;
using UnityEngine.SceneManagement;

public static class CamHelpers
{
    private static bool _isSubscribedToSceneLoaded = false;
    private static Camera _cachedCam;
    public static Camera Cam
    {
        get
        {
            if (_cachedCam == null) _cachedCam = Camera.main;
            return _cachedCam;
        }
    }

    static CamHelpers()
    {
        if (_isSubscribedToSceneLoaded) return;
        SceneManager.sceneLoaded += OnSceneLoaded;
        _isSubscribedToSceneLoaded = true;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _cachedCam = Camera.main;
    }

    public static Vector3 GetCamFlatForward()
    {
        Vector3 camFlatForward = Cam.transform.forward;
        camFlatForward.y = 0f; // flatten to XZ
        camFlatForward.Normalize();
        return camFlatForward;
    }
}
