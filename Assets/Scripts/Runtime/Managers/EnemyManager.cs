using Cysharp.Threading.Tasks;
using System.Linq;
using UnityEngine;

public class EnemyManager : Singleton<EnemyManager>
{
    [SerializeField]
    private EnemyData[] _enemyDataArray;

    [SerializeField]
    private float _spawnRadius = 10f;

    private Transform _playerTransform;
    private void Start()
    {
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        LoadAllAndSpawn().Forget();
    }
    private async UniTaskVoid LoadAllAndSpawn()
    {
        bool[] results = await UniTask.WhenAll(
            _enemyDataArray.Select(data => data.LoadPrefabAsync())
        );

        if (results.All(success => success))
            SpawnEnemyRandomAroundPlayer();
        else
            Debug.LogError("One or more enemy prefabs failed to load.");
    }
    private void SpawnEnemyRandomAroundPlayer()
    {
        if (_playerTransform == null)
            _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        int count = _enemyDataArray.Length;
        float angleStep = 360f / count;
        float randomOffset = Random.Range(0f, 360f);

        for (int i = 0; i < count; i++)
        {
            float angle = Mathf.Deg2Rad * (angleStep * i + randomOffset);
            Vector3 spawnPosition = _playerTransform.position + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * _spawnRadius;

            spawnPosition.y = 0;
            EnemyController enemy = FlyweightFactory.Spawn(_enemyDataArray[i]) as EnemyController;
            enemy.FlyweightInit(spawnPosition, Quaternion.identity);
            enemy.SetTarget(_playerTransform);
        }
    }
}
