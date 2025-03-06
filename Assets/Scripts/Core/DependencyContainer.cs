using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A container for managing dependencies using a Service Locator pattern
/// </summary>
public class DependencyContainer : MonoBehaviour
{
    private static DependencyContainer _instance;
    
    // Dictionary to store services by type
    private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
    
    // Flag to check if the container is initialized
    private bool _isInitialized = false;
    
    /// <summary>
    /// Singleton instance accessor
    /// </summary>
    public static DependencyContainer Instance
    {
        get
        {
            if (_instance == null)
            {
                // Try to find existing instance
                _instance = FindObjectOfType<DependencyContainer>();
                
                // If no instance exists, create one
                if (_instance == null)
                {
                    GameObject container = new GameObject("DependencyContainer");
                    _instance = container.AddComponent<DependencyContainer>();
                    Debug.Log("[DependencyContainer] Created new instance");
                }
                else
                {
                    Debug.Log("[DependencyContainer] Found existing instance");
                }
            }
            
            return _instance;
        }
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
        
        Debug.Log("[DependencyContainer] Awake - Container set up successfully");
    }
    
    /// <summary>
    /// Initializes the container with required services
    /// </summary>
    public void Initialize()
    {
        if (_isInitialized)
        {
            Debug.LogWarning("[DependencyContainer] Container already initialized");
            return;
        }
        
        // Register core services
        RegisterCoreServices();
        
        _isInitialized = true;
        Debug.Log("[DependencyContainer] Container initialized");
    }
    
    /// <summary>
    /// Registers core game services needed for most functionality
    /// </summary>
    private void RegisterCoreServices()
    {
        // Example: Find and register player data
        PlayerData playerData = Resources.Load<PlayerData>("GameData/DefaultPlayerData");
        if (playerData != null)
        {
            Register(playerData);
        }
        else
        {
            Debug.LogError("[DependencyContainer] Failed to load PlayerData from Resources");
        }
        
        // Find player controller in scene
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            Register(playerController);
        }
        else
        {
            Debug.LogWarning("[DependencyContainer] PlayerController not found in scene");
        }
        
        // Register other core services as needed
        // ...
    }
    
    /// <summary>
    /// Registers a service instance with the container
    /// </summary>
    /// <typeparam name="T">The type to register this service as</typeparam>
    /// <param name="service">The service instance</param>
    public void Register<T>(T service)
    {
        Type type = typeof(T);
        
        if (_services.ContainsKey(type))
        {
            Debug.LogWarning($"[DependencyContainer] Service of type {type.Name} already registered. Replacing.");
            _services[type] = service;
        }
        else
        {
            _services.Add(type, service);
            Debug.Log($"[DependencyContainer] Registered service: {type.Name}");
        }
    }
    
    /// <summary>
    /// Resolves a service by type
    /// </summary>
    /// <typeparam name="T">The type of service to resolve</typeparam>
    /// <returns>The service instance or null if not found</returns>
    public T Resolve<T>() where T : class
    {
        Type type = typeof(T);
        
        if (_services.TryGetValue(type, out object service))
        {
            return service as T;
        }
        
        Debug.LogWarning($"[DependencyContainer] Service of type {type.Name} not found");
        return null;
    }
    
    /// <summary>
    /// Checks if a service of the specified type is registered
    /// </summary>
    /// <typeparam name="T">The type to check</typeparam>
    /// <returns>True if the service is registered</returns>
    public bool IsRegistered<T>()
    {
        return _services.ContainsKey(typeof(T));
    }
    
    /// <summary>
    /// Removes a service from the container
    /// </summary>
    /// <typeparam name="T">The type of service to remove</typeparam>
    /// <returns>True if successfully removed</returns>
    public bool Unregister<T>()
    {
        Type type = typeof(T);
        
        if (_services.ContainsKey(type))
        {
            _services.Remove(type);
            Debug.Log($"[DependencyContainer] Unregistered service: {type.Name}");
            return true;
        }
        
        return false;
    }
} 