using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : InjectedMonoBehaviour
{
    [Header("Resources")]
    [SerializeField] private PlayerData _defaultPlayerData;
    [Tooltip("Path to the PlayerData in Resources folder")]
    [SerializeField] private string _playerDataResourcePath = "GameData/DefaultPlayerData";
    
    // Game state
    public bool IsGameActive { get; private set; } = false;
    
    // References to other managers
    [SerializeField] private EnemyManager _enemyManager;
    
    // Singleton instance
    private static GameManager _instance;
    
    // Services
    private PlayerController _playerController;
    
    /// <summary>
    /// Singleton instance accessor
    /// </summary>
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();
                
                if (_instance == null)
                {
                    Debug.LogError("GameManager instance not found! Make sure a GameManager is in the scene.");
                }
            }
            
            return _instance;
        }
    }
    
    protected override void InjectDependencies()
    {
        base.InjectDependencies();
    }
    
    private void Awake()
    {
        // Singleton pattern implementation
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Call base implementation to inject dependencies
        base.Awake();
        
        // Manually ensure DependencyContainer exists and is initialized
        DependencyContainer.Instance.Initialize();
        
        // Ensure we have access to managers
        if (_enemyManager == null)
        {
            // Try to resolve from DI container first
            _enemyManager = Resolve<EnemyManager>();
            
            // If not available in container, try to find in scene
            if (_enemyManager == null)
            {
                _enemyManager = FindObjectOfType<EnemyManager>();
                
                // Register if found
                if (_enemyManager != null)
                {
                    Register<EnemyManager>(_enemyManager);
                }
            }
        }
    }

    private void Start()
    {
        // Find player controller if it exists
        _playerController = Resolve<PlayerController>();
        
        if (_playerController == null)
        {
            Debug.LogWarning("[GameManager] PlayerController not found on Start");
        }
        
        StartGame();
    }
    
    public void StartGame()
    {
        IsGameActive = true;
        
        // Initialize enemies
        if (_enemyManager != null)
            _enemyManager.SpawnInitialEnemies();
        else
            Debug.LogError("EnemyManager reference is missing!");
    }
    
    /// <summary>
    /// Gets the current player data
    /// </summary>
    public PlayerData GetPlayerData()
    {
        // First check if we already have a reference
        if (_defaultPlayerData != null)
        {
            return _defaultPlayerData;
        }
        
        // Then try to resolve from container
        var playerData = Resolve<PlayerData>();
        
        if (playerData != null)
        {
            _defaultPlayerData = playerData;
            return _defaultPlayerData;
        }
        
        // Try to get from player controller
        if (_playerController != null && _playerController.PlayerData != null)
        {
            _defaultPlayerData = _playerController.PlayerData;
            return _defaultPlayerData;
        }
        
        Debug.LogError("[GameManager] Could not resolve PlayerData!");
        return null;
    }
    
    /// <summary>
    /// Called when the game is exiting or scene is changing
    /// Cleans up all event registrations
    /// </summary>
    private void OnDestroy()
    {
        // Reset all events when game manager is destroyed
        // This prevents memory leaks and ghost callbacks
        GameEvents.ResetAllEvents();
    }
} 