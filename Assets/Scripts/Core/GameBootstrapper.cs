using UnityEngine;

/// <summary>
/// Handles initialization of game systems at startup
/// </summary>
public class GameBootstrapper : MonoBehaviour
{
    [SerializeField]
    private bool _initializeOnAwake = true;
    
    private void Awake()
    {
        if (_initializeOnAwake)
        {
            InitializeGame();
        }
    }
    
    /// <summary>
    /// Initializes core game systems
    /// </summary>
    public void InitializeGame()
    {
        // Initialize dependency container first
        InitializeDependencyContainer();
        
        // Other initialization steps can be added here
        
        Debug.Log("[GameBootstrapper] Game initialized successfully");
    }
    
    /// <summary>
    /// Sets up the dependency container
    /// </summary>
    private void InitializeDependencyContainer()
    {
        // Access instance to ensure it's created
        DependencyContainer container = DependencyContainer.Instance;
        
        // Initialize the container
        container.Initialize();
        
        Debug.Log("[GameBootstrapper] Dependency container initialized");
    }
} 