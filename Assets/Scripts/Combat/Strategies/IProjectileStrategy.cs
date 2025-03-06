using UnityEngine;

/// <summary>
/// Interface for projectile firing strategies
/// </summary>
public interface IProjectileStrategy
{
    /// <summary>
    /// Fires projectile(s) according to the strategy implementation
    /// </summary>
    /// <param name="weaponController">Reference to the weapon controller</param>
    /// <param name="position">Starting position</param>
    /// <param name="direction">Direction to fire</param>
    /// <param name="speed">Projectile speed</param>
    /// <param name="damage">Projectile damage</param>
    void FireProjectile(WeaponController weaponController, Vector3 position, Vector3 direction, float speed, float damage);
} 