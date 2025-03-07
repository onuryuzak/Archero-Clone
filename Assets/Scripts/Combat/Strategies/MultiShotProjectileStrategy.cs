using UnityEngine;

/// <summary>
/// Projectile strategy that fires multiple projectiles in a spread pattern
/// </summary>
public class MultiShotProjectileStrategy : IProjectileStrategy
{
    private int _projectileCount = 2; // Default number of projectiles to fire
    private float _spreadAngle = 15f; // Angle between projectiles in degrees
    
    /// <summary>
    /// Constructor with parameters for customization
    /// </summary>
    /// <param name="count">Number of projectiles to fire</param>
    /// <param name="angle">Angle between projectiles</param>
    public MultiShotProjectileStrategy(int count = 2, float angle = 15f)
    {
        _projectileCount = Mathf.Max(1, count); // Ensure at least 1 projectile
        _spreadAngle = angle;
    }
    
    /// <summary>
    /// Constructor that uses data from PlayerData
    /// </summary>
    /// <param name="playerData">Player data to initialize from</param>
    public MultiShotProjectileStrategy(PlayerData playerData)
    {
        if (playerData != null)
        {
            _projectileCount = Mathf.Max(1, playerData.DefaultProjectileCount);
            _spreadAngle = playerData.DefaultSpreadAngle;
        }
        else
        {
            // Try to get player data from DependencyContainer
            var container = DependencyContainer.Instance;
            if (container != null)
            {
                var resolvedPlayerData = container.Resolve<PlayerData>();
                if (resolvedPlayerData != null)
                {
                    _projectileCount = Mathf.Max(1, resolvedPlayerData.DefaultProjectileCount);
                    _spreadAngle = resolvedPlayerData.DefaultSpreadAngle;
                    Debug.Log($"[MultiShotProjectileStrategy] Successfully resolved PlayerData from DependencyContainer");
                    return;
                }
            }
            
            // Try to get from GameManager as a last resort
            var gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                var managerPlayerData = gameManager.GetPlayerData();
                if (managerPlayerData != null)
                {
                    _projectileCount = Mathf.Max(1, managerPlayerData.DefaultProjectileCount);
                    _spreadAngle = managerPlayerData.DefaultSpreadAngle;
                    Debug.Log($"[MultiShotProjectileStrategy] Successfully resolved PlayerData from GameManager");
                    return;
                }
            }
            
            Debug.LogWarning("PlayerData was null in MultiShotProjectileStrategy constructor. Using default values.");
            _projectileCount = 2;
            _spreadAngle = 15f;
        }
    }
    
    /// <summary>
    /// Fires multiple projectiles in a spread pattern
    /// </summary>
    public void FireProjectile(WeaponController weaponController, Vector3 position, Vector3 direction, float speed, float damage)
    {
        // Mermi sayısının en az 2 olduğundan emin ol
        int projectileCount = Mathf.Max(2, _projectileCount);
        
        Debug.Log($"[MultiShotProjectileStrategy] FireProjectile called with projectileCount: {projectileCount}");
        
        // Calculate starting rotation offset to center the spread
        float startAngle = -_spreadAngle * (projectileCount - 1) / 2f;
        Debug.Log($"[MultiShotProjectileStrategy] Start angle: {startAngle}, Spread angle: {_spreadAngle}");
        
        // Create projectiles in spread pattern
        for (int i = 0; i < projectileCount; i++)
        {
            // Calculate angle for this projectile
            float angle = startAngle + _spreadAngle * i;
            
            // Rotate direction vector by angle
            Vector3 rotatedDirection = Quaternion.Euler(0, angle, 0) * direction;
            Debug.Log($"[MultiShotProjectileStrategy] Creating projectile {i+1}/{projectileCount}, angle: {angle}, direction: {rotatedDirection}");
            
            // Create projectile
            GameObject projectile = weaponController.CreateProjectile(position, rotatedDirection, speed, damage);
            if (projectile != null)
            {
                Debug.Log($"[MultiShotProjectileStrategy] Successfully created projectile {i+1}");
            }
            else
            {
                Debug.LogError($"[MultiShotProjectileStrategy] Failed to create projectile {i+1}!");
            }
        }
        
        Debug.Log($"[MultiShotProjectileStrategy] Finished firing {projectileCount} projectiles");
    }
} 