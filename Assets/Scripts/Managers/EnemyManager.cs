using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private int _minEnemyCount = 5;
    [SerializeField] private float _spawnDelay = 1.0f;
    
    [Header("Spawn Area")]
    [SerializeField] private float _spawnAreaWidth = 8f;
    [SerializeField] private float _spawnAreaHeight = 12f;
    [SerializeField] private float _minDistanceFromPlayer = 3f;
    
    private List<Enemy> _activeEnemies = new List<Enemy>();
    private Transform _playerTransform;
    
    private void Start()
    {
        // Find player reference
        _playerTransform = FindObjectOfType<PlayerController>()?.transform;
        if (_playerTransform == null)
            Debug.LogError("PlayerController not found in the scene!");
    }
    
    public void SpawnInitialEnemies()
    {
        for (int i = 0; i < _minEnemyCount; i++)
        {
            SpawnEnemy();
        }
    }
    
    public void SpawnEnemy()
    {
        Vector3 spawnPosition = GetRandomSpawnPosition();
        GameObject enemyObject = Instantiate(_enemyPrefab, spawnPosition, Quaternion.identity);
        Enemy enemy = enemyObject.GetComponent<Enemy>();
        
        if (enemy != null)
        {
            enemy.Initialize();
            enemy.OnEnemyDefeated += HandleEnemyDefeated;
            _activeEnemies.Add(enemy);
        }
        else
        {
            Debug.LogError("Enemy component not found on enemy prefab!");
        }
    }
    
    private Vector3 GetRandomSpawnPosition()
    {
        // Try to find a spawn position that's not too close to the player
        for (int attempts = 0; attempts < 10; attempts++)
        {
            float x = Random.Range(-_spawnAreaWidth / 2, _spawnAreaWidth / 2);
            float z = Random.Range(-_spawnAreaHeight / 2, _spawnAreaHeight / 2);
            Vector3 position = new Vector3(x, 1, z);
            
            // Check if this position is far enough from the player
            if (_playerTransform != null)
            {
                float distanceToPlayer = Vector3.Distance(position, _playerTransform.position);
                if (distanceToPlayer >= _minDistanceFromPlayer)
                {
                    return position;
                }
            }
            else
            {
                // If player reference is missing, just return the position
                return position;
            }
        }
        
        // If we can't find a good position after several attempts, just use a random one
        return new Vector3(
            Random.Range(-_spawnAreaWidth / 2, _spawnAreaWidth / 2),
            0,
            Random.Range(-_spawnAreaHeight / 2, _spawnAreaHeight / 2)
        );
    }
    
    private void HandleEnemyDefeated(Enemy enemy)
    {
        // Remove event subscription
        enemy.OnEnemyDefeated -= HandleEnemyDefeated;
        
        // Remove from active list
        _activeEnemies.Remove(enemy);
        
        // Spawn a new enemy after delay
        StartCoroutine(SpawnEnemyWithDelay());
    }
    
    private IEnumerator SpawnEnemyWithDelay()
    {
        yield return new WaitForSeconds(_spawnDelay);
        SpawnEnemy();
    }
} 