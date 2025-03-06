using UnityEngine;


public class ProjectileStrategyFactory
{

    public static IProjectileStrategy CreateStrategy(GameEnums.ProjectileStrategyType type, object[] parameters = null)
    {
        switch (type)
        {
            case GameEnums.ProjectileStrategyType.Standard:
                return new StandardProjectileStrategy();
                
            case GameEnums.ProjectileStrategyType.MultiShot:
                int projectileCount = parameters != null && parameters.Length > 0 ? (int)parameters[0] : 3;
                float spreadAngle = parameters != null && parameters.Length > 1 ? (float)parameters[1] : 15f;
                return new MultiShotProjectileStrategy(projectileCount, spreadAngle);
            
            case GameEnums.ProjectileStrategyType.Bouncing:
                int bounceCount = parameters != null && parameters.Length > 0 ? (int)parameters[0] : 1;
                float damageFalloff = parameters != null && parameters.Length > 1 ? (float)parameters[1] : 0.25f;
                return new BouncingProjectileStrategy(bounceCount, damageFalloff);
                
            case GameEnums.ProjectileStrategyType.Burning:

                Debug.LogWarning("Burning projectile strategy not implemented yet. Using standard instead.");
                return new StandardProjectileStrategy();
                
            case GameEnums.ProjectileStrategyType.Piercing:

                Debug.LogWarning("Piercing projectile strategy not implemented yet. Using standard instead.");
                return new StandardProjectileStrategy();
                
            case GameEnums.ProjectileStrategyType.Homing:

                Debug.LogWarning("Homing projectile strategy not implemented yet. Using standard instead.");
                return new StandardProjectileStrategy();
                
            case GameEnums.ProjectileStrategyType.Explosive:

                Debug.LogWarning("Explosive projectile strategy not implemented yet. Using standard instead.");
                return new StandardProjectileStrategy();
                
            default:
                return new StandardProjectileStrategy();
        }
    }
} 