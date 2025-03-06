using UnityEngine;

/// <summary>
/// Base class for MonoBehaviours that use dependency injection
/// </summary>
public abstract class InjectedMonoBehaviour : MonoBehaviour
{
    /// <summary>
    /// Flag to track if dependencies have been injected
    /// </summary>
    protected bool DependenciesInjected { get; protected set; } = false;
    
    /// <summary>
    /// Called in Awake to inject dependencies
    /// </summary>
    protected virtual void InjectDependencies()
    {
        // Override in derived classes to inject specific dependencies
        DependenciesInjected = true;
    }
    
    /// <summary>
    /// Wrapper for MonoBehaviour's Awake, ensures dependencies are injected first
    /// </summary>
    protected virtual void Awake()
    {
        InjectDependencies();
        
        if (!DependenciesInjected)
        {
            Debug.LogWarning($"[{GetType().Name}] Dependencies not properly injected in Awake!");
        }
    }
    
    /// <summary>
    /// Resolves a dependency from the DependencyContainer
    /// </summary>
    /// <typeparam name="T">The type of dependency to resolve</typeparam>
    /// <returns>The resolved service or null if not found</returns>
    protected T Resolve<T>() where T : class
    {
        return this.ResolveDependency<T>();
    }
    
    /// <summary>
    /// Attempts to resolve a dependency and assigns it to the provided reference
    /// </summary>
    /// <typeparam name="T">The type of dependency to resolve</typeparam>
    /// <param name="reference">Reference to assign the resolved dependency to</param>
    /// <returns>True if the dependency was successfully resolved and assigned</returns>
    protected bool TryResolve<T>(ref T reference) where T : class
    {
        return this.TryResolveDependency(ref reference);
    }
    
    /// <summary>
    /// Registers a service with the DependencyContainer
    /// </summary>
    /// <typeparam name="T">The type to register this service as</typeparam>
    /// <param name="service">The service to register</param>
    protected void Register<T>(T service)
    {
        this.RegisterDependency(service);
    }
    
    /// <summary>
    /// Validates that a required dependency is not null
    /// </summary>
    /// <param name="dependency">The dependency to validate</param>
    /// <param name="dependencyName">The name of the dependency for error messages</param>
    /// <returns>True if the dependency is valid (not null)</returns>
    protected bool ValidateDependency(object dependency, string dependencyName)
    {
        if (dependency == null)
        {
            Debug.LogError($"[{GetType().Name}] Required dependency '{dependencyName}' is null!");
            return false;
        }
        return true;
    }
} 