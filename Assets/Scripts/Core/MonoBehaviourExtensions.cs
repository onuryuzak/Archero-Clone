using UnityEngine;

/// <summary>
/// Extension methods for MonoBehaviour to simplify dependency management
/// </summary>
public static class MonoBehaviourExtensions
{
    /// <summary>
    /// Resolves a dependency from the DependencyContainer
    /// </summary>
    /// <typeparam name="T">The type of dependency to resolve</typeparam>
    /// <param name="monoBehaviour">The MonoBehaviour instance</param>
    /// <returns>The resolved service or null if not found</returns>
    public static T ResolveDependency<T>(this MonoBehaviour monoBehaviour) where T : class
    {
        return DependencyContainer.Instance.Resolve<T>();
    }
    
    /// <summary>
    /// Attempts to resolve a dependency and assigns it to the provided reference
    /// </summary>
    /// <typeparam name="T">The type of dependency to resolve</typeparam>
    /// <param name="monoBehaviour">The MonoBehaviour instance</param>
    /// <param name="reference">Reference to assign the resolved dependency to</param>
    /// <returns>True if the dependency was successfully resolved and assigned</returns>
    public static bool TryResolveDependency<T>(this MonoBehaviour monoBehaviour, ref T reference) where T : class
    {
        reference = DependencyContainer.Instance.Resolve<T>();
        return reference != null;
    }
    
    /// <summary>
    /// Registers a service with the DependencyContainer
    /// </summary>
    /// <typeparam name="T">The type to register this service as</typeparam>
    /// <param name="monoBehaviour">The MonoBehaviour instance</param>
    /// <param name="service">The service to register</param>
    public static void RegisterDependency<T>(this MonoBehaviour monoBehaviour, T service)
    {
        DependencyContainer.Instance.Register(service);
    }
    
    /// <summary>
    /// Checks if a dependency is registered
    /// </summary>
    /// <typeparam name="T">The type to check</typeparam>
    /// <param name="monoBehaviour">The MonoBehaviour instance</param>
    /// <returns>True if the dependency is registered</returns>
    public static bool IsDependencyRegistered<T>(this MonoBehaviour monoBehaviour)
    {
        return DependencyContainer.Instance.IsRegistered<T>();
    }
} 