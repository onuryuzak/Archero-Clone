using UnityEngine;

/// <summary>
/// Projectile strategy that increases attack speed
/// </summary>
public class AttackSpeedProjectileStrategy : IProjectileStrategy
{
    private float _speedMultiplier = 2f; // Default speed multiplier
    private WeaponController _weaponController; // Reference to weapon controller
    
    /// <summary>
    /// Constructor with parameters for customization
    /// </summary>
    /// <param name="speedMultiplier">Attack speed multiplier</param>
    public AttackSpeedProjectileStrategy(float speedMultiplier = 2f)
    {
        _speedMultiplier = Mathf.Max(1f, speedMultiplier); // Ensure multiplier is at least 1
    }
    
    /// <summary>
    /// Fires a projectile with standard properties but applies attack speed boost
    /// </summary>
    public void FireProjectile(WeaponController weaponController, Vector3 position, Vector3 direction, float speed, float damage)
    {
        // Store reference to weapon controller
        _weaponController = weaponController;
        
        // Apply attack speed multiplier
        _weaponController.SetAttackRateMultiplier(_speedMultiplier);
        
        // Create a standard projectile
        weaponController.CreateProjectile(position, direction, speed, damage);
    }
} 