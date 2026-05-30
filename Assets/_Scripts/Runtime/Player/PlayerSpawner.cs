using Cysharp.Threading.Tasks;
using LokiInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PlayerSpawner : MonoBehaviour
{
    [TabGroup("Character"), Required, SerializeField]
    private CharacterInfos _characterInfos;

    [TabGroup("Spawn"), Required, SerializeField]
    private Vector3 _spawnPosition;

    private AsyncOperationHandle<GameObject> _modelHandle;
    private PlayerController _spawnedPlayer;

    private void Start()
    {
        SpawnPlayerAsync().Forget();
    }

    private async UniTaskVoid SpawnPlayerAsync()
    {
        AssetReference modelRef = _characterInfos.CharacterModelRef;
        if (modelRef == null || !modelRef.RuntimeKeyIsValid())
        {
            Debug.LogError($"[PlayerSpawner] CharacterInfos '{_characterInfos.name}' has no valid character model AssetReference.");
            return;
        }

        _modelHandle = Addressables.LoadAssetAsync<GameObject>(modelRef);
        await _modelHandle.ToUniTask();
        if (_modelHandle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"[PlayerSpawner] Failed to load character model for '{_characterInfos.name}'.");
            return;
        }

        GameObject instance = Instantiate(_modelHandle.Result, _spawnPosition, Quaternion.identity);
        _spawnedPlayer = instance.GetComponent<PlayerController>();
        if (_spawnedPlayer == null)
        {
            Debug.LogError($"[PlayerSpawner] Character model prefab '{_modelHandle.Result.name}' is missing a PlayerController component on its root.");
            return;
        }

        _spawnedPlayer.Initialize(_characterInfos);

        WeaponData defaultWeapon = _characterInfos.DefaultWeapon;
        if (defaultWeapon != null)
            await defaultWeapon.LoadWeaponAssetsAsync();

        EventBus<PlayerSpawnedEvent>.Raise(new PlayerSpawnedEvent(_spawnedPlayer.transform, _characterInfos));

        if (defaultWeapon != null)
            EventBus<AddWeaponEvent>.Raise(new AddWeaponEvent(defaultWeapon));
    }

    private void OnDestroy()
    {
        if (_modelHandle.IsValid())
            Addressables.Release(_modelHandle);
    }
}
