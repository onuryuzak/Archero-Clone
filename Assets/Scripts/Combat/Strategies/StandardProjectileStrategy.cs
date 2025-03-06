using UnityEngine;

/// <summary>
/// Standard projectile strategy that fires a single projectile
/// </summary>
public class StandardProjectileStrategy : IProjectileStrategy
{
    /// <summary>
    /// Fires a single projectile in the specified direction
    /// </summary>
    public void FireProjectile(WeaponController weaponController, Vector3 position, Vector3 direction, float speed, float damage)
    {
        // Create a single projectile
        weaponController.CreateProjectile(position, direction, speed, damage);
    }
} 