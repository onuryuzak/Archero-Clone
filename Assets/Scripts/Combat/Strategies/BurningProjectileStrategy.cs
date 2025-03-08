using UnityEngine;

/// <summary>
/// Projectile strategy that creates projectiles with burning damage effect
/// </summary>
public class BurningProjectileStrategy : IProjectileStrategy
{
    private float _burnDamagePerSecond = 5f; // Default damage per second
    private float _burnDuration = 3f; // Default duration in seconds
    private int _maxStacks = 3; // Default max stacks
    
    /// <summary>
    /// Constructor with parameters for customization
    /// </summary>
    /// <param name="damagePerSecond">Burn damage per second</param>
    /// <param name="duration">Burn duration in seconds</param>
    /// <param name="maxStacks">Maximum number of burn stacks</param>
    public BurningProjectileStrategy(float damagePerSecond = 5f, float duration = 3f, int maxStacks = 3)
    {
        _burnDamagePerSecond = Mathf.Max(0f, damagePerSecond);
        _burnDuration = Mathf.Max(0f, duration);
        _maxStacks = Mathf.Max(1, maxStacks);
    }
    
    /// <summary>
    /// Fires a projectile that applies burning effect to targets
    /// </summary>
    public void FireProjectile(WeaponController weaponController, Vector3 position, Vector3 direction, float speed, float damage)
    {
        // Create the projectile
        GameObject projectileObj = weaponController.CreateProjectile(position, direction, speed, damage);
        
        // Add burning component if projectile was created successfully
        if (projectileObj != null)
        {
            BurningProjectile burningProjectile = projectileObj.GetComponent<BurningProjectile>();
            if (burningProjectile == null)
            {
                burningProjectile = projectileObj.AddComponent<BurningProjectile>();
            }
            
            // Configure the burning properties
            burningProjectile.Initialize(_burnDamagePerSecond, _burnDuration, _maxStacks);
        }
    }
} 