using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [Header("Enemy Settings")] [SerializeField]
    private GameObject _enemyPrefab;

    [SerializeField] private int _minEnemyCount = 5;
    [SerializeField] private int _maxEnemyCount = 5; // Maksimum düşman sayısı
    [SerializeField] private float _spawnDelay = 1.0f;

    [Header("Spawn Area")] [SerializeField]
    private float _spawnAreaWidth = 8f;

    [SerializeField] private float _spawnAreaHeight = 12f;

    private List<Enemy> _activeEnemies = new List<Enemy>();
    private Transform _playerTransform;

    private void OnEnable()
    {
        // Event aboneliğini bir kez yapalım
        GameEvents.OnEnemyDefeated += HandleEnemyDefeated;
    }

    private void OnDisable()
    {
        // Event aboneliğini temizleyelim
        GameEvents.OnEnemyDefeated -= HandleEnemyDefeated;
    }

    private void Start()
    {
        // Find player reference
        _playerTransform = FindFirstObjectByType<PlayerController>()?.transform;
        if (_playerTransform == null)
            Debug.LogError("PlayerController not found in the scene!");
    }

    public void SpawnInitialEnemies()
    {
        // Başlangıçta düşman sayımız _minEnemyCount kadar olsun ve maksimum sayıyı geçmesin
        int spawnCount = Mathf.Min(_minEnemyCount, _maxEnemyCount);
        for (int i = 0; i < spawnCount; i++)
        {
            SpawnEnemy();
        }
    }

    public void SpawnEnemy()
    {
        // Maksimum düşman sayısını kontrol et
        if (_activeEnemies.Count >= _maxEnemyCount)
        {
            Debug.Log($"[EnemyManager] Maximum enemy count ({_maxEnemyCount}) reached. Not spawning more enemies.");
            return;
        }

        Vector3 spawnPosition = GetRandomSpawnPosition();
        GameObject enemyObject = Instantiate(_enemyPrefab, spawnPosition, Quaternion.identity);
        Enemy enemy = enemyObject.GetComponent<Enemy>();

        if (enemy != null)
        {
            enemy.Initialize();
            _activeEnemies.Add(enemy);
            Debug.Log($"[EnemyManager] Enemy spawned. Active enemy count: {_activeEnemies.Count}/{_maxEnemyCount}");
        }
        else
        {
            Debug.LogError("Enemy component not found on enemy prefab!");
        }
    }

    private Vector3 GetRandomSpawnPosition()
    {
        return new Vector3(
            Random.Range(-_spawnAreaWidth / 2, _spawnAreaWidth / 2),
            1,
            Random.Range(-_spawnAreaHeight / 2, _spawnAreaHeight / 2)
        );
    }

    private void HandleEnemyDefeated(Enemy enemy)
    {
        // Aktif düşman listesinden çıkar
        if (_activeEnemies.Contains(enemy))
        {
            _activeEnemies.Remove(enemy);
            Debug.Log($"[EnemyManager] Enemy defeated. Active enemy count: {_activeEnemies.Count}/{_maxEnemyCount}");

            // Yeni düşman oluştur (gecikme ile)
            StartCoroutine(SpawnEnemyWithDelay());
        }
    }

    private IEnumerator SpawnEnemyWithDelay()
    {
        yield return new WaitForSeconds(_spawnDelay);
        SpawnEnemy();
    }
}