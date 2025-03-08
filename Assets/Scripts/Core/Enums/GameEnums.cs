/// <summary>
/// Oyundaki tüm enum tanımlamalarını içerir
/// </summary>
public static class GameEnums
{
    /// <summary>
    /// Defines types of skills
    /// </summary>
    public enum SkillType
    {
        ArrowMultiplication,
        BounceDamage,
        BurnDamage,
        AttackSpeedIncrease,
        RageMode
    }
    
    /// <summary>
    /// Defines types of projectile strategies
    /// </summary>
    public enum ProjectileStrategyType
    {
        Standard,
        MultiShot,
        Bouncing,
        Burning,
        AttackSpeed
    }
} 