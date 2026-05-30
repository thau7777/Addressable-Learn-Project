using Unity.Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private CinemachineCamera _cinemachineCamera;

    private EventBinding<PlayerSpawnedEvent> _playerSpawnedBinding;
    private void Awake()
    {
        _cinemachineCamera = GetComponent<CinemachineCamera>();
    }

    private void OnEnable()
    {
        _playerSpawnedBinding = new EventBinding<PlayerSpawnedEvent>(OnPlayerSpawned);
        EventBus<PlayerSpawnedEvent>.Register(_playerSpawnedBinding);
    }
    private void OnDisable()
    {
        EventBus<PlayerSpawnedEvent>.Deregister(_playerSpawnedBinding);
    }

    private void OnPlayerSpawned(PlayerSpawnedEvent evt)
    {
        _cinemachineCamera.Follow = evt.PlayerTransform;
        _cinemachineCamera.LookAt = evt.PlayerTransform;
    }
}
