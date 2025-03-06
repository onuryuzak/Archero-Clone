using UnityEngine;

/// <summary>
/// Projectile strategy that creates projectiles with the ability to bounce between targets
/// </summary>
public class BouncingProjectileStrategy : IProjectileStrategy
{
    private int _bounceCount = 1; // Default number of bounces
    private float _damageFalloff = 0.25f; // Damage reduction per bounce (percentage)
    
    /// <summary>
    /// Constructor with parameters for customization
    /// </summary>
    /// <param name="bounces">Number of bounces</param>
    /// <param name="falloff">Damage falloff per bounce (0 to 1)</param>
    public BouncingProjectileStrategy(int bounces = 1, float falloff = 0.25f)
    {
        _bounceCount = Mathf.Max(0, bounces);
        _damageFalloff = Mathf.Clamp01(falloff); // Ensure falloff is between 0 and 1
    }
    
    /// <summary>
    /// Fires a projectile that can bounce between targets
    /// </summary>
    public void FireProjectile(WeaponController weaponController, Vector3 position, Vector3 direction, float speed, float damage)
    {
        // Create the projectile
        GameObject projectileObj = weaponController.CreateProjectile(position, direction, speed, damage);
        
        // Add bouncing component if bounce count > 0
        if (_bounceCount > 0 && projectileObj != null)
        {
            BouncingProjectile bouncer = projectileObj.GetComponent<BouncingProjectile>();
            if (bouncer == null)
            {
                bouncer = projectileObj.AddComponent<BouncingProjectile>();
            }
            
            // Configure the bouncing properties
            bouncer.Initialize(_bounceCount, _damageFalloff, damage);
        }
    }
} 