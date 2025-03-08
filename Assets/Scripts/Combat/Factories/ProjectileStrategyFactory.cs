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
                float burnDamagePerSecond = parameters != null && parameters.Length > 0 ? (float)parameters[0] : 5f;
                float burnDuration = parameters != null && parameters.Length > 1 ? (float)parameters[1] : 3f;
                int maxStacks = parameters != null && parameters.Length > 2 ? (int)parameters[2] : 3;
                return new BurningProjectileStrategy(burnDamagePerSecond, burnDuration, maxStacks);
                
            case GameEnums.ProjectileStrategyType.AttackSpeed:
                float speedMultiplier = parameters != null && parameters.Length > 0 ? (float)parameters[0] : 2f;
                return new AttackSpeedProjectileStrategy(speedMultiplier);
                
            default:
                return new StandardProjectileStrategy();
        }
    }
} 