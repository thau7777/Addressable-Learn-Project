using UnityEngine;
using UnityEngine.SceneManagement;
public enum ActionMap
{
    Gameplay,
    UI,
}

[CreateAssetMenu(fileName = "InputReader", menuName = "Scriptable Objects/InputReader")]
public class InputReader : ScriptableObject
{
    private MyInputActions input;
    public GameplayActions gameplayActions;

    private void OnEnable()
    {
        InitializeActions();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        DisableActions();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Additive) return;

        switch (scene.name)
        {
            case "Gameplay":
                SwitchActionMap(ActionMap.Gameplay);
                break;
            default:
                Debug.LogWarning($"Scene {scene.name} does not have a defined action map. Defaulting to PlayerTopDown.");
                break;
        }
    }
    private void InitializeActions()
    {
        gameplayActions = new GameplayActions();
        if (input == null)
        {
            input = new MyInputActions();
            input.Gameplay.SetCallbacks(gameplayActions);
        }
    }
    public void DisableActions()
    {
        if(input == null) return;
        input.Gameplay.Disable();
    }
    public void SwitchActionMap(ActionMap map)
    {
        DisableActions();
        switch (map)
        {
            case ActionMap.Gameplay:
                input.Gameplay.Enable();
                break;
            default:
                Debug.LogWarning("ActionMap not recognized: " + map.ToString());
                break;
        }
        Debug.Log("switched actionButton map to: " + map.ToString());
    }
}
